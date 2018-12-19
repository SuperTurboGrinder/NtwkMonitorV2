export class MassPingCancellator {
    private _isCancelled = false;
    public onCancel: () => void = null;

    cancel() {
        if (this.onCancel !== null) {
            this.onCancel();
        }
        this._isCancelled = true;
    }

    get isCancelled(): boolean {
        return this._isCancelled;
    }
}
