import { powerSaveBlocker, app, BrowserWindow } from 'electron';
import { join } from 'path';
import { WebAPIProcessService } from './webAPIProcessService';

const monitorAPIStarter = new WebAPIProcessService();


app.once('window-all-closed', () => app.quit());
app.once('ready', () => {
    monitorAPIStarter.execAfterAPIStarted((ok: boolean) => {
        if (ok === true) {
            console.log('Starting electron app.');
            let window = new BrowserWindow({
                minWidth: 800,
                minHeight: 600
            });
            window.maximize();
            window.once('closed', () => { window = null; });
            window.loadURL('file://' + join(__dirname, '../index.html'));
            // window.webContents.openDevTools();
            powerSaveBlocker.start('prevent-app-suspension');
        } else {
            console.log('Quiting electron app.');
            app.quit();
        }
    });
});
