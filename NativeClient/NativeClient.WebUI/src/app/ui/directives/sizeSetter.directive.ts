import { Directive, HostBinding, Input } from "@angular/core";

@Directive({
    selector: ".nmSizeSetter"
})
export class SizeSetterDirective {
    @HostBinding("style.width")
    width:string;

    @HostBinding("style.height")
    height:string;

    setWidth(width:string) {
        this.width = width+"px";
    }

    setHeight(height:string) {
        this.height = height+"px";
    }
}