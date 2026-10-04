import { useState } from "react";
import {
  Bar,
  CartesianGrid,
  ComposedChart,
  Legend,
  Line,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import type { DailySummary } from "../api/stockApp";
import { formatChartDay, formatCompactNumber } from "../utils/Format";

interface SummaryChartProps {
  summaries: DailySummary[];
}

type ChartRange = "1W" | "2W" | "1M";

const chartRanges: { label: ChartRange; days: number | null }[] = [
  { label: "1W", days: 7 },
  { label: "2W", days: 14 },
  { label: "1M", days: null },
];

export default function SummaryChart({ summaries }: SummaryChartProps) {
  const [selectedRange, setSelectedRange] = useState<ChartRange>("1M");
  const selectedDays = chartRanges.find(
    (range) => range.label === selectedRange,
  )?.days;
  const visibleSummaries =
    selectedDays === null || selectedDays === undefined
      ? summaries
      : summaries.slice(-selectedDays);

  return (
    <div aria-label="Daily low and high average prices with volume">
      <div className="chart-range-controls" role="group" aria-label="Chart date range">
        {chartRanges.map(({ label }) => (
          <button
            key={label}
            type="button"
            className={selectedRange === label ? "active" : ""}
            aria-pressed={selectedRange === label}
            onClick={() => setSelectedRange(label)}
          >
            {label}
          </button>
        ))}
      </div>
      <ResponsiveContainer width="100%" height={360}>
        <ComposedChart
          data={visibleSummaries}
          margin={{ top: 12, right: 12, bottom: 8, left: 12 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="date"
            tickFormatter={formatChartDay}
            minTickGap={24}
          />
          <YAxis
            yAxisId="price"
            tickFormatter={formatCompactNumber}
            width={72}
          />
          <YAxis
            yAxisId="volume"
            orientation="right"
            tickFormatter={formatCompactNumber}
            width={72}
          />
          <Tooltip
            labelFormatter={(label) => formatChartDay(String(label))}
            formatter={(value, name) => [
              typeof value === "number" ? value.toString() : String(value),
              String(name),
            ]}
          />
          <Legend />
          <Bar
            yAxisId="volume"
            dataKey="totalVolume"
            name="Volume"
            fill="#94a3b8"
            fillOpacity={0.55}
          />
          <Line
            yAxisId="price"
            type="monotone"
            dataKey="lowAverage"
            name="Low average"
            stroke="#2563eb"
            strokeWidth={2}
            dot={false}
            activeDot={{ r: 5 }}
          />
          <Line
            yAxisId="price"
            type="monotone"
            dataKey="highAverage"
            name="High average"
            stroke="#ea580c"
            strokeWidth={2}
            dot={false}
            activeDot={{ r: 5 }}
          />
        </ComposedChart>
      </ResponsiveContainer>
    </div>
  );
}
