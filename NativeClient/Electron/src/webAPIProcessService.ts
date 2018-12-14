import { exec, spawn } from 'child_process';
import { normalize } from 'path';

export class WebAPIProcessService {
    private readonly processName = 'NativeClient.WebAPI';
    private ext = '';
    // https://stackoverflow.com/questions/38033127/node-js-how-to-check-a-process-is-running-by-the-process-name
    private checkIfProcessIsRunning(
        query: string,
        callback: (result: boolean, failed: boolean) => void
    ) {
        const platform = process.platform;
        let cmd = '';
        switch (platform) {
            case 'win32' :
                cmd = `tasklist`;
                this.ext = '.exe';
                break;
            case 'darwin' : cmd = `ps -ax | grep ${query}`; break;
            case 'linux' : cmd = `ps -A`; break;
            default: break;
        }
        exec(cmd, (err, stdout, stderr) => {
            if (err) { callback(false, true); }
            else {
                callback(
                    stdout.toLowerCase()
                        .indexOf(query.toLowerCase()+this.ext) > -1,
                    false
                );
            }
        });
    }

    private getAPIExecutableFolder(): string {
        const electronExePath = process.execPath;
        let electronExeFolder: string = null;
        let x;
        x = electronExePath.lastIndexOf('/');
        if (x >= 0) { // Unix-based path
            electronExeFolder = electronExePath.substr(0, x+1);
        }
        x = electronExePath.lastIndexOf('\\');
        if (x >= 0) { // Windows-based path
            electronExeFolder = electronExePath.substr(0, x+1);
        }
        return normalize(electronExeFolder+'/WebAPI/');
    }

    execAfterAPIStarted(callback: (ok: boolean) => void) {
        this.checkIfProcessIsRunning(
            this.processName,
            (result, failed) => {
                if (failed === true) {
                    console.log('Failed to check if Monitor API is running.');
                    callback(false);
                } else {
                    if (result === false) {
                        const exeFolder = this.getAPIExecutableFolder();
                        const filePath = exeFolder+this.processName+this.ext;
                        console.log(`Opening ${filePath} executable`);
                        const apiProcess = spawn(filePath, [], {
                            detached: true,
                            cwd: exeFolder
                        });
                        apiProcess.stdout.on('data', (data) => {
                            console.log(`API server stdout: ${data}`);
                        });
                        setTimeout(() => {
                            callback(true);
                        }, 5000);
                    } else {
                        console.log('Monitor API is runnging.');
                        callback(true);
                    }
                }
            }
        );
    }
}