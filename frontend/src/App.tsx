import { useDailySummary } from "./hooks/useDailySummary";
import SymbolForm from "./components/SymbolForm";
import StatusMessage from "./components/StatusMessage";
import SummaryTable from "./components/SummaryTable";
import SummaryChart from "./components/SummaryChart";

export default function App() {
  const { state, load } = useDailySummary();

  return (
    <main className="app-shell">
      <header className="app-header">
        <a className="brand" href="/" aria-label="MarketScope home">
          <span className="brand-mark" aria-hidden="true">M</span>
          <span>MarketScope</span>
        </a>
        <span className="header-caption">Market data dashboard</span>
      </header>

      <section className="search-panel" aria-labelledby="page-title">
        <p className="eyebrow">MARKET INSIGHTS</p>
        <h1 id="page-title">Daily stock performance</h1>
        <p className="page-description">
          Explore daily price averages and trading volume for the past month.
        </p>
        <SymbolForm
          onSearch={load}
          isLoading={state.status === "loading"}
        />
      </section>

      <div className="status-container">
        <StatusMessage state={state} />
      </div>

      {state.status === "done" && state.data.length > 0 && (
        <section className="summary-section" aria-live="polite">
          <div className="summary-heading">
            <div>
              <p className="eyebrow">PERFORMANCE OVERVIEW</p>
              <h2>{state.symbol}</h2>
            </div>
            <span className="period-badge">Past month</span>
          </div>
          <div className="chart-card">
            <SummaryChart summaries={state.data} />
          </div>
          <div className="table-card">
            <SummaryTable summaries={state.data} />
          </div>
        </section>
      )}
      <footer className="app-footer">Market data for informational purposes</footer>
    </main>
  );
}