import { Component, Output, EventEmitter, Input } from "@angular/core";

export class Range {
    constructor(
        public value: number,
        public length: number
    ) {}
    
    public fit(max_value, max_length) {
        let higher_value = this.value + this.length -1;
        let oldVal = this.value;
        let oldLen = this.length;
        this.value = this.value <= max_value
            ? this.value : max_value;
        this.length = higher_value <= max_value
            ? this.length <= max_length ? this.length : max_length
            : max_value - this.value + 1;
        let changed = this.value !== oldVal || this.length !== oldLen;
        return changed;
    }

    public incr() {
        this.value++;
        this.shorten();
    }
    
    public decr() {
        this.value--;
        this.lengthen();
    }

    public lengthen() {
        this.length++;
    }

    public shorten() {
        this.length = (this.length > 1) ? this.length-1 : 1;
    }
}

@Component({
    selector: 'numRangeSelector',
    templateUrl: './numRangeSelector.component.html'
})
export class NumRangeSelectorComponent {
    private _range: Range = new Range(0, 1);
    private _max_value: number = 0;
    private _max_length: number = 1;
    private _initialized = false;
    
    @Output() private changedEvent = new EventEmitter<Range>();

    private emitChange() {
        this.changedEvent.emit(this._range);
    }

    private fit() {
        let changed = this._range.fit(this._max_value, this._max_length);
        if(changed) {
            this.emitChange();
        }
    }

    public currentLength() {
        return this._range.length;
    }

    public ofMax() {
        return this._max_value+1;
    }

    @Input() set maxValue(newMax: number) {
        this._max_value = newMax;
        this.fit();
    }

    @Input() set maxLength(maxLength: number) {
        this._max_length = maxLength;
        this._range.length = maxLength;
        this.fit();
    }

    @Input() set initialValue(val: Range) {
        if(val === null) return;
        if(this._initialized === false) {
            this._initialized = true;
            this._range = val;
        }
    }

    public isLowerDownActive(): boolean {
        return this._range.value !== 0
        && this._range.length !== this._max_length;
    }

    public isLowerUpActive(): boolean {
        return this._range.value !== this._max_value
        && this._range.length > 1;
    }

    public isHigherDownActive(): boolean {
        return this._range.length > 1;
    }

    public isHigherUpActive(): boolean {
        return this._range.length < this._max_length
        && this.higherValue() < this._max_value;
    }

    public lowerValue(): number {
        return this._range.value;
    }

    public higherValue(): number {
        return this._range.value + this._range.length -1;
    }

    public decrValue() {
        if(this.isLowerDownActive()) {
            this._range.decr();
            this.emitChange();
        }
    }

    public incrValue() {
        if(this.isLowerUpActive()) {
            this._range.incr();
            this.emitChange();
        }
    }

    public decrLength() {
        if(this.isHigherDownActive()) {
            this._range.shorten();
            this.emitChange();
        }
    }

    public incrLength() {
        if(this.isHigherUpActive()) {
            this._range.lengthen();
            this.emitChange();
        }
    }
}