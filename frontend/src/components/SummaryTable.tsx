import type { DailySummary } from "../api/stockApp";
import { formatDay, formatPrice, formatVolume } from "../utils/Format";

interface SummaryTableProps {
  summaries: DailySummary[];
}

export default function SummaryTable({ summaries }: SummaryTableProps) {
  return (
    <table className="summary-table">
      <caption>Daily stock summary</caption>
      <thead>
        <tr>
          <th scope="col">Day</th>
          <th scope="col">Low average</th>
          <th scope="col">High average</th>
          <th scope="col">Volume</th>
        </tr>
      </thead>
      <tbody>
        {summaries.map((summary) => (
          <tr key={summary.date}>
            <th scope="row">{formatDay(summary.date)}</th>
            <td>{formatPrice(summary.lowAverage)}</td>
            <td>{formatPrice(summary.highAverage)}</td>
            <td>{formatVolume(summary.totalVolume)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}