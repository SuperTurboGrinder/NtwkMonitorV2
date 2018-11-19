export class ManualTimer {
    private lastResetTime: number = null;

    constructor(
        private msInterval: number
    ) {}

    public getMsToReset(): number {
        return this.msInterval - (Date.now() - this.lastResetTime);
    }

    public checkIfFinished(): boolean {
        if (this.lastResetTime === null) {
            this.Reset();
            return true;
        } else {
            if (this.getMsToReset() <= 0) {
                this.Reset();
                return true;
            } else {
                return false;
            }
        }
    }

    private Reset() {
        this.lastResetTime = Date.now();
    }
}
