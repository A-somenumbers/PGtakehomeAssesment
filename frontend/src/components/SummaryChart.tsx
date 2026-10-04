import {
  Bar,
  Brush,
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

export default function SummaryChart({ summaries }: SummaryChartProps) {
  return (
    <div role="img" aria-label="Daily low and high average prices with volume">
      <ResponsiveContainer width="100%" height={360}>
        <ComposedChart
          data={summaries}
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
          <Brush
            dataKey="date"
            height={28}
            travellerWidth={10}
            tickFormatter={formatChartDay}
            ariaLabel="Select the date range to zoom the chart"
          />
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
