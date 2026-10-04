export type ApiErrorKind =
  | "invalid-symbol"
  | "not-found"
  | "upstream"
  | "request"
  | "invalid-response";

export class ApiError extends Error {
  public readonly kind: ApiErrorKind;
  public readonly status?: number;

  constructor(
    message: string,
    kind: ApiErrorKind,
    status?: number,
  ) {
    super(message);
    this.name = "ApiError";
    this.kind = kind;
    this.status = status;
  }
}

export async function createApiError(response: Response): Promise<ApiError> {
  const problem = await response.json().catch(() => null) as {
    title?: unknown;
    detail?: unknown;
  } | null;

  const message =
    (typeof problem?.detail === "string" && problem.detail) ||
    (typeof problem?.title === "string" && problem.title) ||
    `The request failed with status ${response.status}.`;

  if (response.status === 400) {
    return new ApiError(message, "invalid-symbol", response.status);
  }

  if (response.status === 404) {
    return new ApiError(message, "not-found", response.status);
  }

  if (response.status === 502) {
    return new ApiError(message, "upstream", response.status);
  }

  return new ApiError(message, "request", response.status);
}
