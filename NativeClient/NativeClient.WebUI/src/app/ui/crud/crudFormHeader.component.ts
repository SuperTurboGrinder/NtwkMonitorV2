import { Component, Input } from "@angular/core";

@Component({
    selector: 'crudFormHeader',
    templateUrl: './crudFormHeader.component.html'
})
export class CrudFormHeaderComponent {
    @Input() isEditMode: boolean = false;
}