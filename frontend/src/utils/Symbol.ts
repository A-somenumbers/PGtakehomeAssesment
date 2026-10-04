const SYMBOL_PATTERN = /^[A-Za-z0-9.^=_-]{1,32}$/;

export function isValidSymbol(symbol: string): boolean {
  return SYMBOL_PATTERN.test(symbol.trim());
}