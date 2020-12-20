// <![CDATA[
var xhr = new XMLHttpRequest();
function btn(cmdSend) {
    xhr.open('GET', cmdSend + '&_=' + Math.random());
    xhr.send();
}
// ]]>