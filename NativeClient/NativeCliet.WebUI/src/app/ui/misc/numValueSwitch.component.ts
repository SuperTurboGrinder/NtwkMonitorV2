import { Component, Output, EventEmitter, Input } from "@angular/core";

@Component({
    selector: 'numValueSwitch',
    templateUrl: './numValueSwitch.component.html'
})
export class NumValueSwitchComponent {
    private _min: number = 0;
    private _max: number = 0;
    private _value: number = 0;
    
    @Output() private changedValueEvent = new EventEmitter<number>();

    set value(newValue: number) {
        this._value = newValue;
        this.changedValueEvent.emit(newValue);
    }

    @Input() set min(newMin: number) {
        console.log("min changed")
        this._min = newMin;
        if(this.value < newMin) {
            this.value = newMin;
        }
    }

    @Input() set max(newMax: number) {
        console.log("max changed")
        this._max = newMax;
        if(this.value > newMax) {
            this.value = newMax;
        }
    }

    get value(): number { return this._value }
    get min(): number { return this._min }
    get max(): number { return this._max }

    isMin(): boolean {
        return this.value === this.min;
    }

    isMax(): boolean {
        return this.value === this.max;
    }

    increment() {
        if(!this.isMax())
            this.value++;
    }

    decrement() {
        if(!this.isMin())
            this.value--;
    }
}