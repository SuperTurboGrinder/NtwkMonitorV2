export class HTTPResult<T> {
    private constructor(
        public data: T = null,
        public success = false
    ) {}

    public static Successful<T>(_data: T) : HTTPResult<T> {
        return new HTTPResult<T>(_data, true);
    }

    public static Failed<T>() : HTTPResult<T> {
        return new HTTPResult<T>();
    }
}