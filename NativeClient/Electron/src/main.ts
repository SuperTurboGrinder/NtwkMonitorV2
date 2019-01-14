import { app, BrowserWindow, powerSaveBlocker } from 'electron';
import { join } from 'path';
import { WebAPIProcessService } from './webAPIProcessService';

let electronWindow: BrowserWindow = null;

const shouldQuit = app.makeSingleInstance((_1, _2) => {
  if (electronWindow) {
    if (electronWindow.isMinimized()) electronWindow.restore();
    electronWindow.focus();
  }
});

if (shouldQuit) {
  app.quit();
}

const monitorAPIStarter = new WebAPIProcessService();
app.once('window-all-closed', () => app.quit());
app.once('ready', () => {
    monitorAPIStarter.execAfterAPIStarted((ok: boolean) => {
        if (ok === true) {
            console.log('Starting electron app.');
            electronWindow = new BrowserWindow({
                minWidth: 800,
                minHeight: 600,
                icon: join(__dirname, '../icon.png'),
                show: false
            });
            electronWindow.once('ready-to-show', () => {
                electronWindow.maximize();
            });
            electronWindow.once('closed', () => { electronWindow = null; });
            electronWindow.loadURL('file://' + join(__dirname, '../index.html'));
            // electronWindow.webContents.openDevTools();
            powerSaveBlocker.start('prevent-app-suspension');
        } else {
            console.log('Quiting electron app.');
            app.quit();
        }
    });
});
