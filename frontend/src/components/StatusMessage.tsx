import type { DailySummaryState } from "../hooks/useDailySummary";

interface StatusMessageProps {
  state: DailySummaryState;
}

export default function StatusMessage({ state }: StatusMessageProps) {
  switch (state.status) {
    case "idle":
      return <p>Enter a stock symbol to view its daily summary.</p>;
    case "loading":
      return <p role="status">Loading {state.symbol}...</p>;
    case "done":
      return state.data.length === 0 ? (
        <p>No data found for {state.symbol}.</p>
      ) : null;
    case "fail":
      return <p role="alert">{state.error.message}</p>;
  }
}