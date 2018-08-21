import { Messages } from "../servicesModel/messagesEnum.model"
import { BackendErrorStatuses } from "../httpModel/backendErrorStatuses.model"

export class MessageToDisplay {
    public isMessage:boolean;
    public message: Messages;
    public errorStatus: BackendErrorStatuses;
    public time: Date;
    public isOld;
}