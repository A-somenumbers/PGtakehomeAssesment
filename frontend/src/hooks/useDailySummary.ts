import { useCallback, useEffect, useRef, useState } from "react";
import { ApiError } from "../api/errors";
import { fetchDailySummary, type DailySummary } from "../api/stockApp";

export type DailySummaryState =
  | { status: "idle" }
  | { status: "loading"; symbol: string }
  | { status: "done"; symbol: string; data: DailySummary[] }
  | { status: "fail"; symbol: string; error: ApiError };

export interface UseDailySummaryResult {
  state: DailySummaryState;
  load: (symbol: string) => void;
}

export function useDailySummary(): UseDailySummaryResult {
  const [state, setState] = useState<DailySummaryState>({ status: "idle" });
  const controllerRef = useRef<AbortController | null>(null);

  const load = useCallback((symbol: string) => {
    controllerRef.current?.abort();

    const controller = new AbortController();
    controllerRef.current = controller;
    const normalizedSymbol = symbol.trim().toUpperCase();
    setState({ status: "loading", symbol: normalizedSymbol });

    void fetchDailySummary(normalizedSymbol, controller.signal)
      .then((data) => {
        if (!controller.signal.aborted) {
          setState({ status: "done", symbol: normalizedSymbol, data });
        }
      })
      .catch((error: unknown) => {
        if (controller.signal.aborted) {
          return;
        }

        const apiError =
          error instanceof ApiError
            ? error
            : new ApiError(
                "An unexpected error occurred while loading stock data.",
                "request",
              );
        setState({ status: "fail", symbol: normalizedSymbol, error: apiError });
      });
  }, []);

  useEffect(
    () => () => {
      controllerRef.current?.abort();
    },
    [],
  );

  return { state, load };
}