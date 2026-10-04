import { ApiError, createApiError } from "./errors";

export interface DailySummary {
  date: string;
  lowAverage: number;
  highAverage: number;
  totalVolume: number;
}

const apiBaseUrl = (import.meta.env.VITE_API_URL ?? "").replace(/\/+$/, "");

export async function fetchDailySummary(
  symbol: string,
  signal?: AbortSignal,
): Promise<DailySummary[]> {
  let response: Response;

  try {
    response = await fetch(
      `${apiBaseUrl}/api/stocks/${encodeURIComponent(symbol)}/daily-summary`,
      {
        headers: { Accept: "application/json" },
        signal,
      },
    );
  } catch (error) {
    if (error instanceof DOMException && error.name === "AbortError") {
      throw error;
    }

    throw new ApiError(
      "Could not reach the stock data service. Check your connection and try again.",
      "request",
    );
  }

  if (!response.ok) {
    throw await createApiError(response);
  }

  try {
    const summaries: unknown = await response.json();
    if (
      !Array.isArray(summaries) ||
      !summaries.every(isDailySummary)
    ) {
      throw new Error("Unexpected response shape.");
    }

    return summaries;
  } catch (error) {
    if (error instanceof ApiError) {
      throw error;
    }

    throw new ApiError(
      "The stock data service returned an invalid response.",
      "invalid-response",
    );
  }
}

function isDailySummary(value: unknown): value is DailySummary {
  if (typeof value !== "object" || value === null) {
    return false;
  }

  const summary = value as Record<string, unknown>;
  return (
    typeof summary.date === "string" &&
    typeof summary.lowAverage === "number" &&
    Number.isFinite(summary.lowAverage) &&
    typeof summary.highAverage === "number" &&
    Number.isFinite(summary.highAverage) &&
    typeof summary.totalVolume === "number" &&
    Number.isSafeInteger(summary.totalVolume)
  );
}
