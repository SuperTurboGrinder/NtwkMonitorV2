
export class HTTPResult<T> {
    constructor(
        public data: T,
        public error: string = null
    ) {}

    isSuccess(): boolean {
        return this.error === null;
    }
}

export class ExtHttpClient {
    private static readonly baseApiURL = 'http://localhost:5001/api/';

    private requestToAPI<TFrom, T=TFrom>(
        path: string,
        method: string,
        callback: (result: HTTPResult<XMLHttpRequest>) => void,
        body: TFrom = null
    ) {
        const request = new XMLHttpRequest();
        request.open(method, ExtHttpClient.baseApiURL+path, true);
        request.setRequestHeader("Content-type", "application/json");
        request.onload = () => callback(new HTTPResult(request));
        request.onerror = () => callback(
            this.buildError(
                this.parseError(request)
            )
        );
        request.send(JSON.stringify(body));
    }

    private buildError<T>(errstr: string): HTTPResult<T> {
        console.error(errstr);
        return new HTTPResult<T>(null, errstr);
    }
    
    private parseError(xmlRequest: XMLHttpRequest): string {
        if (xmlRequest.status === 500) {
            return 'Backend server returned code 500 (internal error).';
        } else if (xmlRequest.status === 400) {
            const errResponse = JSON.parse(xmlRequest.responseText);
            return `${errResponse.status}: ${errResponse.statusString}`;
        } else {
            return `ExtHttpClient error: ${xmlRequest.status}`;
        }
    }

    dataRequest<TFrom, T=TFrom>(
        path: string,
        method: string,
        callback: (result: HTTPResult<T>) => void,
        body: TFrom = null
    ) {
        this.requestToAPI(path, method, result => {
            if (result.isSuccess()) {
                if (result.data.status === 204) {
                    callback(this.buildError("Status 204 when expected 200"));
                } else {
                    callback(new HTTPResult<T>(JSON.parse(result.data.responseText)));
                }
            }
        }, body);
    }

    rawDataRequest<TFrom>(
        path: string,
        method: string,
        callback: (result: HTTPResult<string>) => void,
        body: TFrom = null
    ) {
        this.requestToAPI(path, method, result => {
            if (result.isSuccess()) {
                if (result.data.status === 204) {
                    callback(this.buildError("Status 204 when expected 200"));
                } else {
                    callback(new HTTPResult<string>(result.data.responseText));
                }
            }
        }, body);
    }

    operationRequest<TFrom>(
        path: string,
        method: string,
        callback: (result: HTTPResult<boolean>) => void,
        body: TFrom = null
    ) {
        this.requestToAPI(path, method, result => {
            if (result.isSuccess()) {
                if (result.data.status === 200) {
                    callback(this.buildError("Status 200 when expected 204"));
                } else {
                    callback(new HTTPResult(true));
                }
            }
        }, body);
    }
}
