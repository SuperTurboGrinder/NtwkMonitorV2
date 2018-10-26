import { HTTPResult } from 'src/app/model/servicesModel/httpResult.model';
import { MessagingService } from 'src/app/services/messaging.service';
import { MessagesEnum } from 'src/app/model/servicesModel/messagesEnum.model';

export abstract class BaseCrudSelectorComponent<DataType, DataService> {
    private data: DataType[] = null;
    private loadingError = false;
    public displayOperationInProgress = false;

    constructor(
        private messager: MessagingService,
        protected dataService: DataService
    ) {
        this.updateDataList();
    }

    protected abstract updateDataList();

    protected setNewData(dataResult: HTTPResult<DataType[]>) {
        if (dataResult.success === true) {
            this.data = dataResult.data;
        } else {
            this.loadingError = true;
        }
    }

    public get dataList(): DataType[] {
        return this.data;
    }

    public get isLoadingError(): boolean {
        return this.loadingError;
    }

    public refresh(_: boolean) {
        this.updateDataList();
    }

    protected abstract deleteObjectPermanently(
        objID: number,
        callback: (success: boolean) => void
    );

    protected confirmOperationSuccess(
        success: boolean,
        successMessage: MessagesEnum
    ) {
        if (success === true) {
            this.data = null;
            this.updateDataList();
            this.messager.showMessage(successMessage);
        }
        this.displayOperationInProgress = false;
    }

    public deleteObject(event: {shouldDelete: boolean, id: number}) {
        if (event.shouldDelete) {
            this.displayOperationInProgress = true;
            this.deleteObjectPermanently(
                event.id,
                (success: boolean) => this.confirmOperationSuccess(
                    success,
                    MessagesEnum.DeletedSuccessfully
                )
            );
        }
    }
}
