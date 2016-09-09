// HCVncCore.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include "HCVncCore.h"

CPPCallback* m_CallBack_DEBUG;
CPPCallback* m_CallBack;
CPPUpdateCallback* m_CallUpdateBack;

DSVncViewerAdapter::DSVncViewerAdapter()
{

}

DSVncViewerAdapter::~DSVncViewerAdapter()
{

}


void DSVncViewerAdapter::start()
{
	if (m_viewerCore) {
		delete m_viewerCore;
		m_viewerCore = NULL;
	}

	m_CallBack_DEBUG("准备进行实例化");
	m_viewerCore = new RemoteViewerCore(0);
	
	PixelFormat pixelFormat;
	pixelFormat.initBigEndianByNative();
	pixelFormat.bitsPerPixel = 32;
	pixelFormat.blueMax = 255;
	pixelFormat.blueShift = 0;
	pixelFormat.greenMax = 255;
	pixelFormat.greenShift = 8;
	pixelFormat.redMax = 255;
	pixelFormat.redShift = 16;
	pixelFormat.colorDepth = 32;
	m_viewerCore->setPixelFormat(&pixelFormat);
	m_viewerCore->allowCopyRect(true);
	m_viewerCore->setPreferredEncoding(EncodingDefs::TIGHT);
	m_viewerCore->setCompressionLevel(6);
	m_viewerCore->setJpegQualityLevel(6);
	m_viewerCore->enableCursorShapeUpdates(false);
	m_viewerCore->ignoreCursorShapeUpdates(false);

	m_viewerCore->start(m_ip, 5901, this, true);
	m_CallBack_DEBUG("实例化完毕");
}

void DSVncViewerAdapter::stop()
{
	m_viewerCore->stopUpdating(true);
	m_viewerCore->stop();
}

void DSVncViewerAdapter::refresh()
{
	m_viewerCore->setPreferredEncoding(EncodingDefs::TIGHT);
}

void DSVncViewerAdapter::onBell()
{
	m_CallBack_DEBUG("onBell\n");
}

void DSVncViewerAdapter::onCutText(const StringStorage *cutText)
{

}

void DSVncViewerAdapter::onEstablished()
{
	m_CallBack_DEBUG("onEstablished\n");
}

void DSVncViewerAdapter::onConnected(RfbOutputGate *output)
{
	m_CallBack_DEBUG("connect\n");
	m_CallBack("success");
}

void DSVncViewerAdapter::onDisconnect(const StringStorage *message)
{
	m_CallBack_DEBUG("onDisconnect\n");
	m_CallBack("close");
}

void DSVncViewerAdapter::onAuthError(const AuthException *exception)
{
	m_CallBack_DEBUG("onAuthError\n");
	m_CallBack("error");
}

void DSVncViewerAdapter::onError(const Exception *exception)
{
	m_CallBack_DEBUG("onError\n");
	m_CallBack("error");
}

int m_dswidth = 0;
int m_dsheight = 0;

void DSVncViewerAdapter::onFrameBufferUpdate(const FrameBuffer *fb, const Rect *update)
{
	AutoLock al(&m_bufferLock);
	if (!m_framebuffer.copyFrom(update, fb, update->left, update->top)) {
		m_CallBack_DEBUG("复制缓冲区数据失败\n");
	}

	int left = update->left;
	int top = update->top;
	int right = update->right;
	int bottom = update->bottom;

	m_CallUpdateBack((UCHAR *)m_framebuffer.getBufferPtr(0, 0), m_framebuffer.getBufferSize(), m_dswidth, m_dsheight, left, top, right, bottom);
}

void DSVncViewerAdapter::onFrameBufferPropChange(const FrameBuffer *fb)
{
	Dimension dimension = fb->getDimension();
	Dimension olddimension = m_framebuffer.getDimension();

	AutoLock al(&m_bufferLock);

	m_width = dimension.width;
	m_height = dimension.height;
	m_bitsPerPixel = fb->getBitsPerPixel();

	if (!dimension.isEmpty()) {
		int alignWidth = (dimension.width + 3) / 4;
		dimension.width = alignWidth * 4;
		int alignHeight = (dimension.height + 3) / 4;
		dimension.height = alignHeight * 4;
		m_framebuffer.setProperties(&dimension,
			&fb->getPixelFormat());
		m_framebuffer.setColor(0, 0, 0);
	}
	else {
		m_framebuffer.setColor(0, 0, 0);
	}

	m_dswidth = dimension.width;
	m_dsheight = dimension.height;
}


//实现设置回调函数的方法
DSVNCLIBRARY_API void SetCallback_DEBUG(CPPCallback_DEBUG callback)
{
	m_CallBack_DEBUG = callback;
}

//实现设置回调函数的方法
DSVNCLIBRARY_API void SetCallback(CPPCallback callback)
{
	m_CallBack = callback;
}

//实现设置回调函数的方法
DSVNCLIBRARY_API void SetUpdateCallback(CPPUpdateCallback callback)
{
	m_CallUpdateBack = callback;
}

DSVncViewerAdapter a;
//启动VNC客户端
DSVNCLIBRARY_API void StartVNCClient()
{
	m_CallBack_DEBUG("准备进行启动VNC客户端的逻辑");
	a.start();
}

//关闭VNC客户端
DSVNCLIBRARY_API void CloseVNCClient()
{
	m_CallBack_DEBUG("准备关闭VNC客户端");
	a.stop();
}

//设置推屏IP
DSVNCLIBRARY_API void SetIP(TCHAR* ip)
{
	m_CallBack_DEBUG("配置准备推屏的IP");
	a.m_ip = ip;
}
