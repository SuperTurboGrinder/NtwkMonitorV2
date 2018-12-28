import { exec, spawn } from 'child_process';
import { normalize } from 'path';
import { net } from 'electron';

export class WebAPIProcessService {
    private readonly processName = 'NetMonV2.APIServer';
    private started = false;
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
                        let counter = 0;
                        const check = () => {
                            console.log('Checking for server response...');
                            if (this.started || ++counter === 10) {
                                console.log('Finished waiting for responses.');
                                callback(this.started);
                            } else {
                                setTimeout(
                                    () => this.setStartedIfResponding(check),
                                    3000
                                );
                            }
                        };
                        check();
                    } else {
                        console.log('Monitor API is runnging.');
                        callback(true);
                    }
                }
            }
        );
    }

    private setStartedIfResponding(finished: () => void) {
        const request = net.request({
            method: 'GET',
            protocol: 'http:',
            hostname: 'localhost',
            port: 5438,
            path: '/api/nodes'
        });
        request.on('response', (response) => {
            console.log(`API server responded with status ${response.statusCode}`);
            this.started = true;
            finished();
        });
        request.on('error', (error) => {
            console.log('Request to API server failed:');
            console.log(error);
            finished();
        });
        request.end();
    }
}