function GetCookie(sName, defaultValue) {
    var aCookie = document.cookie.split("; ");
    for (var i = 0; i < aCookie.length; i++) {
        var aCrumb = aCookie[i].split("=");
        if (sName == aCrumb[0]) return unescape(aCrumb[1])
    }
    return defaultValue
}
function SetCookie(name, value, expires, path, domain, secure) {
    var today = new Date;
    today.setTime(today.getTime());
    if (expires) expires = expires * 1000 * 60 * 60 * 24;
    var expires_date = new Date(today.getTime() + expires);
	var cookie = name + "=" + escape(value) + (expires ? ";expires=" + expires_date.toGMTString() : "") + (path ? ";path=" + path : "") + (domain ? ";domain=" + domain : "") + (secure ? ";secure" : "");
    document.cookie = cookie;
}
var tocWidthIndex = 1, tocWidths = [0, 280, 380, 480];
initToc();

function initToc() {
    tocWidthIndex = GetCookie("tableOfContentsWidthIndex", 1);
	resizeToc();
}
function onIncreaseToc() {
    tocWidthIndex++;
    if (tocWidthIndex > 3) tocWidthIndex = 0;
    resizeToc();
    SetCookie("tableOfContentsWidthIndex", tocWidthIndex, 365, "/", ".varigence.com", null)
}
function onResetToc() {
    tocWidthIndex = 0;
    resizeToc();
    SetCookie("tableOfContentsWidthIndex", tocWidthIndex, 365, "/", ".varigence.com", null)
}
function resizeToc() {
    var toc = document.getElementById("tableOfContents");
    if (toc) {
        var width = tocWidths[tocWidthIndex];
        toc.style.width = width + "px";
        if (width == 0) 
            window.setTimeout(function () { toc.style.display = "none" }, 0);
        else 
            toc.style.display = "block";
        //document.getElementById("tableOfContentsResizeThumb").style.left = width + "px";
        document.getElementById("ResizeImageExpand").style.display = tocWidthIndex == tocWidths.length - 1 ? "none" : "";
        document.getElementById("ResizeImageMinimize").style.display = tocWidthIndex != tocWidths.length - 1 ? "none" : ""
    }
}