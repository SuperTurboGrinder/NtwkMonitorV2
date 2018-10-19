import { Location } from '@angular/common';
import { ActivatedRouteSnapshot, ActivatedRoute } from '@angular/router';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';
import { MessagingService } from 'src/app/services/messaging.service';

export abstract class BaseCrudFormComponent<DataType, DataService> {
    public readonly isEditMode: boolean;
    private readonly originalDataID: number;
    private originalData: DataType = null;
    public data: DataType = null;

    public displayOperationInProgress = true;
    public isLoadingError = false;
    public displayDiscardMessage = false;
    public displayCreateConfirmationMessage = false;
    public displaySaveChangesMessage = false;

    constructor(
        private messager: MessagingService,
        private location: Location,
        route: ActivatedRoute,
        protected dataService: DataService
    ) {
        const routeSnapshot: ActivatedRouteSnapshot = route.snapshot;
        this.isEditMode = routeSnapshot.url[1].path === 'edit';
        if (this.isEditMode) {
            this.originalDataID = parseInt(routeSnapshot.params.id, 10);
            this.reloadOriginalData();
        } else {
            this.originalData = this.newEmptyData();
            this.data = this.newEmptyData();
            this.displayOperationInProgress = false;
        }
    }

    protected abstract getOriginalData(
        id: number,
        callback: (success: boolean, orig: DataType) => void
    );
    protected abstract newEmptyData(): DataType;
    protected abstract currentIdenticalTo(data: DataType): boolean;
    protected abstract makeCopy(orig: DataType): DataType;
    protected abstract saveAsNewObjectInDatabase(
        callback: (success: boolean) => void
    );
    protected abstract saveChangesToObjectInDatabase(
        callback: (success: boolean) => void
    );

    public refresh(_: boolean) {
        this.reloadOriginalData();
    }

    private reloadOriginalData() {
        this.displayOperationInProgress = true;
        this.getOriginalData(
            this.originalDataID,
            (success, orig) => {
                this.isLoadingError = success === false;
                if (success) {
                    this.displayOperationInProgress = false;
                    this.originalData = orig;
                    this.data = this.makeCopy(this.originalData);
                }
            }
        );
    }

    public tryDiscard() {
        console.log(this.data);
        console.log(this.originalData);
        if (this.currentIdenticalTo(this.originalData)) {
            this.discardAndReturn(true);
        } else {
            this.displayDiscardMessage = true;
        }
    }

    public discardAndReturn(shouldDiscard: boolean) {
        this.displayDiscardMessage = false;
        if (shouldDiscard) {
            this.location.back();
        }
    }

    private confirmOperationSuccess(
        success: boolean,
        successMessage: MessagesEnum
    ) {
        if (success) {
            this.messager.showMessage(successMessage);
            this.location.back();
        } else {
            this.displayOperationInProgress = false;
        }
    }

    public tryCreateNew() {
        this.displayCreateConfirmationMessage = true;
    }

    public createNewObjectAndReturn(shouldCreate: boolean) {
        this.displayCreateConfirmationMessage = false;
        if (shouldCreate) {
            this.displayOperationInProgress = true;
            this.saveAsNewObjectInDatabase(
                (success: boolean) => this.confirmOperationSuccess(
                    success,
                    MessagesEnum.CreatedSuccessfully
                )
            );
        }
    }

    public trySaveChanges() {
        if (this.currentIdenticalTo(this.originalData)) {
            this.messager.showMessage(MessagesEnum.NoChangesDetected);
        } else {
            this.displaySaveChangesMessage = true;
        }
    }

    public saveChangesToObjectAndReturn(shouldSave: boolean) {
        this.displaySaveChangesMessage = false;
        if (shouldSave) {
            this.displayOperationInProgress = true;
            this.saveChangesToObjectInDatabase(
                (success: boolean) => this.confirmOperationSuccess(
                    success,
                    MessagesEnum.UpdatedSuccessfully
                )
            );
        }
    }
}
