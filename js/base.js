(function(doc, win) {
	// 分辨率Resolution适配
	var docEl = doc.documentElement,
		resizeEvt = 'orientationchange' in window ? 'orientationchange' : 'resize',
		recalc = function() {
			var clientWidth = docEl.clientWidth;
			if(!clientWidth) return;
			docEl.style.fontSize = 16 * (clientWidth / 320) + 'px';
			docEl.style.width = '100%';
			docEl.style.height = '100%';
			docEl.style.overflow = 'hidden';
		};

	if(!doc.addEventListener) return;
	win.addEventListener(resizeEvt, recalc, false);
	doc.addEventListener('DOMContentLoaded', recalc, false);

	// 一物理像素在不同屏幕的显示效果不一样。要根据devicePixelRatio来修改meta标签的scale,要注释上面的meta标签
	(function() {
		return;
		var dpr = scale = 1;
		var isIPhone = win.navigator.appVersion.match(/iphone/gi);
		var devicePixelRatio = win.devicePixelRatio;
		if(isIPhone) {
			// iOS下，对于2和3的屏，用2倍的方案，其余的用1倍方案
			if(devicePixelRatio >= 3 && (!dpr || dpr >= 3)) {
				dpr = 3;
			} else if(devicePixelRatio >= 2 && (!dpr || dpr >= 2)) {
				dpr = 2;
			} else {
				dpr = 1;
			}
		} else {
			// 其他设备下，仍旧使用1倍的方案
			dpr = 1;
		}
		scale = 1 / dpr;
		var metaEl = "";
		metaEl = doc.createElement('meta');
		metaEl.setAttribute('name', 'viewport');
		metaEl.setAttribute('content', 'initial-scale=' + scale + ', maximum-scale=' + scale + ', minimum-scale=' + scale + ', user-scalable=no');
		if(docEl.firstElementChild) {
			docEl.firstElementChild.appendChild(metaEl);
		} else {
			var wrap = doc.createElement('div');
			wrap.appendChild(metaEl);
			doc.write(wrap.innerHTML);
		}
	})();

})(document, window);

var RootUrl = "http://139.224.51.196/";

function IsNullOrEmpty(str) {
	if(!str) {
		return true;
	}
	return false;
}

/**
 * 格式化缩略图显示
 */
function ShowThumb(url, thumb) {
	if(IsNullOrEmpty(url)) {
		return "img/logo.png";
	}
	if(url.indexOf('_0') < 0) {
		return url;
	}
	return url.replace("_0", "_" + thumb);
}

/**
 * 格式化滚动条
 */
function InitScroll() {
	var deceleration = mui.os.ios ? 0.003 : 0.0009; // 阻尼系数
	mui('.mui-scroll-wrapper').scroll({
		bounce: false,
		indicators: false, // 是否显示滚动条
		deceleration: deceleration
	});
}

/*获取html的参数**********************************************/
function GetParameter(param) {
	var query = window.location.search;
	query = doFilter(query);
	var iLen = param.length;
	var iStart = query.indexOf(param);
	if(iStart == -1) {
		return "";
	}
	iStart += iLen + 1;
	var iEnd = query.indexOf("&", iStart);
	if(iEnd == -1) {
		return query.substring(iStart);
	}
	return query.substring(iStart, iEnd);
};

//过滤多余问号的情况
function doFilter(query) {
	//出现多个？的情况，保留第一个，其他都改为&
	var queryParam = query.split('?');
	if(queryParam.length > 1) {
		var _index = query.indexOf("?") + 1;
		var queryBefore = query.substr(0, _index);
		var queryAfter = query.substr(_index);
		queryAfter = queryAfter.replace("?", "&");
		//替换除第一？外的情况
		query = queryBefore + queryAfter;
	}
	return query;
};

/**
 * 验证权限密码
 */
function CheckPowerPwd(articleid, pwd, callback) {
	mui.getJSON(RootUrl + "Article/CheckPowerPwd", {
		ArticleID: articleid,
		ArticlePowerPwd: pwd
	}, function(data) {
		if(data != null) {
			if(data.result) {
				if(callback) {
					callback();
				}
				return;
			}
		}
		mui.toast("校验失败");
	});
}