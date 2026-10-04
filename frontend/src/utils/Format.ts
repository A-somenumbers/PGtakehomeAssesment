const priceFormatter = new Intl.NumberFormat(undefined, {
  minimumFractionDigits: 4,
  maximumFractionDigits: 4,
});

const volumeFormatter = new Intl.NumberFormat(undefined, {
  maximumFractionDigits: 0,
});

const dayFormatter = new Intl.DateTimeFormat(undefined, {
  weekday: "long",
  month: "long",
  day: "numeric",
  year: "numeric",
  timeZone: "UTC",
});

const chartDayFormatter = new Intl.DateTimeFormat(undefined, {
  month: "short",
  day: "numeric",
  timeZone: "UTC",
});

const compactNumberFormatter = new Intl.NumberFormat(undefined, {
  notation: "compact",
  maximumFractionDigits: 1,
});

export function formatPrice(price: number): string {
  return priceFormatter.format(price);
}

export function formatVolume(volume: number): string {
  return volumeFormatter.format(volume);
}

export function formatDay(day: string): string {
  return dayFormatter.format(new Date(`${day}T00:00:00Z`));
}

export function formatChartDay(day: string): string {
  return chartDayFormatter.format(new Date(`${day}T00:00:00Z`));
}

export function formatCompactNumber(value: number): string {
  return compactNumberFormatter.format(value);
}