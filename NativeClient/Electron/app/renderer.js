const searchInPage = require('electron-in-page-search').default;

let search;
const webview = document.getElementById('target-webview');
webview.addEventListener('dom-ready', () => {
    search = searchInPage(webview);
    this.turnOnDefaultBehaviour = window.addEventListener(
        'keydown',
        e => {
            const f3 = e.keyCode === 114;
            const ctrl_f = e.ctrlKey && e.keyCode === 70;
            if (ctrl_f || f3) {
                e.preventDefault();
                if (ctrl_f) {
                    search.openSearchWindow();
                } else if (f3) {
                    if (e.shiftKey) {
                        search.findNext(false);
                    } else {
                        search.findNext(true);
                    }
                }
            }
        }
    );
});
