import { powerSaveBlocker, app, BrowserWindow } from 'electron';
import { join } from 'path';

app.once('window-all-closed', () => app.quit());

app.once('ready', () => {
    let window = new BrowserWindow({
        minWidth: 800,
        minHeight: 600
    });
    window.once('closed', () => { window = null; });
    window.loadURL('file://' + join(__dirname, 'index.html'));
    window.webContents.openDevTools();
    powerSaveBlocker.start('prevent-app-suspension');
});
