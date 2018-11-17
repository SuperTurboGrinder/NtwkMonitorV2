import { Component, Output, EventEmitter, Input, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'app-num-value-switch',
    templateUrl: './numValueSwitch.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NumValueSwitchComponent {
    private _min = 0;
    private _max = 0;
    private _value = 0;
    private _initialized = false;

    @Output() private changedValueEvent = new EventEmitter<number>();

    private emitChange() {
        this.changedValueEvent.emit(this._value);
    }

    @Input() set min(newMin: number) {
        this._min = newMin;
        if (this.value < newMin) {
            this._value = newMin;
            this.emitChange();
        }
    }

    @Input() set max(newMax: number) {
        this._max = newMax;
        if (this.value > newMax) {
            this._value = newMax;
            this.emitChange();
        }
    }

    @Input() set initialValue(val: number) {
        if (val === null) {
            return;
        }
        if (this._initialized === false) {
            this._initialized = true;
            this._value = val;
            this.emitChange();
        }
    }

    get value(): number { return this._value; }
    get min(): number { return this._min; }
    get max(): number { return this._max; }

    isMin(): boolean {
        return this._value === this._min;
    }

    isMax(): boolean {
        return this._value === this._max;
    }

    increment() {
        if (!this.isMax()) {
            this._value++;
            this.emitChange();
        }
    }

    decrement() {
        if (!this.isMin()) {
            this._value--;
            this.emitChange();
        }
    }
}
