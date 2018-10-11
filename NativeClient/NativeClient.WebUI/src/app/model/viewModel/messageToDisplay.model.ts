import { MessagesEnum } from "../servicesModel/messagesEnum.model"
import { BackendErrorStatuses } from "../httpModel/backendErrorStatuses.model"

export class MessageToDisplay {
    public isMessage:boolean;
    public message: MessagesEnum;
    public errorStatus: BackendErrorStatuses;
    public time: number;
}