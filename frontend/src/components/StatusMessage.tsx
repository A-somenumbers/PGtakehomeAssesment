import type { DailySummaryState } from "../hooks/useDailySummary";

interface StatusMessageProps {
  state: DailySummaryState;
}

export default function StatusMessage({ state }: StatusMessageProps) {
  switch (state.status) {
    case "idle":
      return <p className="status-message">Enter a stock symbol to view its daily summary.</p>;
    case "loading":
      return <p className="status-message" role="status">Loading {state.symbol}...</p>;
    case "done":
      return state.data.length === 0 ? (
        <p className="status-message">No data found for {state.symbol}.</p>
      ) : null;
    case "fail":
      return <p className="status-message status-error" role="alert">{state.error.message}</p>;
  }
}