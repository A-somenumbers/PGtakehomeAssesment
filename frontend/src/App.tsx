import { useDailySummary } from "./hooks/useDailySummary";
import SymbolForm from "./components/SymbolForm";
import StatusMessage from "./components/StatusMessage";
import SummaryTable from "./components/SummaryTable";
import SummaryChart from "./components/SummaryChart";

export default function App() {
  const { state, load } = useDailySummary();

  return (
    <main style={{ padding: 24, fontFamily: "sans-serif" }}>
      <SymbolForm
        onSearch={load}
        isLoading={state.status === "loading"}
      />
      <StatusMessage state={state} />
      {state.status === "done" && state.data.length > 0 && (
        <section aria-live="polite">
          <h2>Daily summary for {state.symbol}</h2>
          <SummaryChart summaries={state.data} />
          <SummaryTable summaries={state.data} />
        </section>
      )}
    </main>
  );
}