import { useState, type FormEvent } from "react";
import { isValidSymbol } from "../utils/Symbol";

interface SymbolFormProps {
  onSearch: (symbol: string) => void;
  isLoading?: boolean;
}

export default function SymbolForm({
  onSearch,
  isLoading = false,
}: SymbolFormProps) {
  const [symbol, setSymbol] = useState("TSLA");
  const [validationError, setValidationError] = useState("");

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const normalizedSymbol = symbol.trim().toUpperCase();

    if (!isValidSymbol(normalizedSymbol)) {
      setValidationError(
        "Symbol must be 1-32 characters: letters, digits, '.', '-', '^', '=', or '_'.",
      );
      return;
    }

    setSymbol(normalizedSymbol);
    setValidationError("");
    onSearch(normalizedSymbol);
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <label htmlFor="stock-symbol">Stock symbol</label>
      <input
        id="stock-symbol"
        value={symbol}
        onChange={(event) => {
          setSymbol(event.target.value);
          setValidationError("");
        }}
        autoComplete="off"
        aria-invalid={validationError !== ""}
        aria-describedby={validationError ? "symbol-error" : undefined}
      />
      <button type="submit" disabled={isLoading || symbol.trim().length === 0}>
        {isLoading ? "Searching..." : "Search"}
      </button>
      {validationError && (
        <p id="symbol-error" role="alert">
          {validationError}
        </p>
      )}
    </form>
  );
}