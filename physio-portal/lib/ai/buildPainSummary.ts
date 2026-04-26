// lib/ai/buildPainSummary.ts
type Region = { label: string; painType: string; intensity: number };

export function buildPainSummary(regions: Region[]) {
  const dominantPainType = regions.reduce((a, b) =>
    b.intensity > a.intensity ? b : a
  ).painType;

  const maxIntensity = Math.max(...regions.map((r) => r.intensity));

  return {
    regions: regions.map(({ label, painType, intensity }) => ({
      label,
      painType,
      intensity,
    })),
    summary: { dominantPainType, maxIntensity },
  };
}