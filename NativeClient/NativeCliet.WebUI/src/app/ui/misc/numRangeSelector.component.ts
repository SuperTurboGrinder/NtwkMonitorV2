import { Component, Output, EventEmitter, Input } from "@angular/core";

@Component({
    selector: 'numRangeSelector',
    templateUrl: './numRangeSelector.component.html'
})
export class NumRangeSelectorComponent {
    private _min: number = 0;
    private _max: number = 0;
    private _lower_value: number = 0;
    private _higher_value: number = 0;
    
    @Output() private changedValueEvent = new EventEmitter<{
        lower: number,
        higher: number
    }>();

    private emitChange() {
        this.changedValueEvent.emit({
            lower: this._lower_value,
            higher: this._higher_value
        });
    }

    setHigher(newHigher: number) {
        this.higher_value = newHigher;
    }
    setLower(newLower: number) {
        this.lower_value = newLower;
    }

    set lower_value(newLowerValue: number) {
        this._lower_value = newLowerValue;
        this.emitChange();
    }

    set higher_value(newHigherValue: number) {
        this._higher_value = newHigherValue;
        this.emitChange();
    }

    get lower_value(): number { return this._lower_value }
    get higher_value(): number { return this._higher_value }
    get min(): number { return this._min; }
    get max(): number { return this._max; }
    get higher_min(): number {
        return this._lower_value
    }
    get lower_max(): number {
        return this._higher_value
    }

    @Input() set min(newMin: number) {
        this._min = newMin;
        if(this.lower_value < newMin) {
            this.lower_value = newMin;
        }
    }

    @Input() set max(newMax: number) {
        this._max = newMax;
        if(this.higher_value > newMax) {
            this.higher_value = newMax;
        }
    }
}