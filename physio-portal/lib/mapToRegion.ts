// // type RegionRule = {
// //   name: string;
// //   xMin: number;
// //   xMax: number;
// //   yMin: number;
// //   yMax: number;
// // };
// // type PainZone = {
// //   uvX: number;
// //   uvY: number;
// //   painType: string;
// //   intensity: number;
// // };
// // const REGION_RULES: RegionRule[] = [
// //   // Lower back
// //   {
// //     name: "Lower back",
// //     xMin: 0.3,
// //     xMax: 0.7,
// //     yMin: 0.6,
// //     yMax: 0.85,
// //   },

import { PainZone, ProcessedRegion } from "@/types";

// //   // Left shoulder
// //   {
// //     name: "Left shoulder",
// //     xMin: 0.0,
// //     xMax: 0.4,
// //     yMin: 0.3,
// //     yMax: 0.6,
// //   },

// //   // Right shoulder
// //   {
// //     name: "Right shoulder",
// //     xMin: 0.6,
// //     xMax: 1.0,
// //     yMin: 0.3,
// //     yMax: 0.6,
// //   },

// //   // Left knee
// //   {
// //     name: "Left knee",
// //     xMin: 0.0,
// //     xMax: 0.4,
// //     yMin: 0.0,
// //     yMax: 0.3,
// //   },

// //   // Right knee
// //   {
// //     name: "Right knee",
// //     xMin: 0.6,
// //     xMax: 1.0,
// //     yMin: 0.0,
// //     yMax: 0.3,
// //   },
// // ];

// // lib/painZoneMapper.ts

// export type PainZone = {
//   zoneId: string;
//   uvX: number;
//   uvY: number;
//   worldPosition?: { x: number; y: number; z: number };
//   painType: string;
//   intensity: number;
//   timestamp: string;
// };

// export type ProcessedZone = {
//   label: string;
//   painType: string;
//   intensity: number;
//   count: number;
//   svgX: number;
//   svgY: number;
//   color: string;
// };

// // type RegionRule = {
// //   name: string;
// //   xMin: number; xMax: number;
// //   yMin: number; yMax: number;
// // };

// // UV origin = bottom-left in Unity.
// // uvY=0 → feet, uvY=1 → head
// // uvX=0 → right side of body (from front), uvX=1 → left side
// // const REGION_RULES: RegionRule[] = [
// //   // ── Head & Neck ──────────────────────────────
// //   { name: "Head",           xMin: 0.30, xMax: 0.70, yMin: 0.88, yMax: 1.00 },
// //   { name: "Neck",           xMin: 0.35, xMax: 0.65, yMin: 0.82, yMax: 0.88 },

// //   // ── Shoulders ────────────────────────────────
// //   { name: "Left shoulder",  xMin: 0.00, xMax: 0.28, yMin: 0.68, yMax: 0.82 },
// //   { name: "Right shoulder", xMin: 0.72, xMax: 1.00, yMin: 0.68, yMax: 0.82 },

// //   // ── Arms ─────────────────────────────────────
// //   { name: "Left upper arm", xMin: 0.00, xMax: 0.22, yMin: 0.55, yMax: 0.68 },
// //   { name: "Right upper arm",xMin: 0.78, xMax: 1.00, yMin: 0.55, yMax: 0.68 },
// //   { name: "Left forearm",   xMin: 0.00, xMax: 0.20, yMin: 0.42, yMax: 0.55 },
// //   { name: "Right forearm",  xMin: 0.80, xMax: 1.00, yMin: 0.42, yMax: 0.55 },

// //   // ── Torso ─────────────────────────────────────
// //   { name: "Upper chest",    xMin: 0.28, xMax: 0.72, yMin: 0.72, yMax: 0.82 },
// //   { name: "Mid back",       xMin: 0.28, xMax: 0.72, yMin: 0.60, yMax: 0.72 },
// //   { name: "Lower back",     xMin: 0.28, xMax: 0.72, yMin: 0.50, yMax: 0.60 },
// //   { name: "Abdomen",        xMin: 0.30, xMax: 0.70, yMin: 0.42, yMax: 0.55 },

// //   // ── Hips ──────────────────────────────────────
// //   { name: "Left hip",       xMin: 0.10, xMax: 0.40, yMin: 0.35, yMax: 0.48 },
// //   { name: "Right hip",      xMin: 0.60, xMax: 0.90, yMin: 0.35, yMax: 0.48 },

// //   // ── Legs ──────────────────────────────────────
// //   { name: "Left thigh",     xMin: 0.10, xMax: 0.42, yMin: 0.22, yMax: 0.38 },
// //   { name: "Right thigh",    xMin: 0.58, xMax: 0.90, yMin: 0.22, yMax: 0.38 },
// //   { name: "Left knee",      xMin: 0.10, xMax: 0.42, yMin: 0.14, yMax: 0.22 },
// //   { name: "Right knee",     xMin: 0.58, xMax: 0.90, yMin: 0.14, yMax: 0.22 },
// //   { name: "Left shin",      xMin: 0.10, xMax: 0.42, yMin: 0.05, yMax: 0.14 },
// //   { name: "Right shin",     xMin: 0.58, xMax: 0.90, yMin: 0.05, yMax: 0.14 },
// // ];
// // lib/painZoneMapper.ts

// export function worldToLabel(pos: { x: number; y: number }): string {
//   const { x, y } = pos;
//   const side = x < -0.08 ? "Left " : x > 0.08 ? "Right " : "";

//   if (y > 1.55)                         return "Head";
//   if (y > 1.42)                         return "Neck";
//   if (y > 1.20 && Math.abs(x) > 0.15)  return `${side}Shoulder`;
//   if (y > 1.05 && Math.abs(x) > 0.15)  return `${side}Upper arm`;
//   if (y > 0.85 && Math.abs(x) > 0.15)  return `${side}Forearm`;
//   if (y > 1.20)                         return "Upper chest";
//   if (y > 1.00)                         return "Mid chest / Upper back";
//   if (y > 0.80)                         return "Lower back";
//   if (y > 0.65)                         return "Abdomen";
//   if (y > 0.50)                         return `${side}Hip`;
//   if (y > 0.35)                         return `${side}Thigh`;
//   if (y > 0.22)                         return `${side}Knee`;
//   if (y > 0.08)                         return `${side}Shin / Calf`;
//   return                                        `${side}Foot`;
// }

// // SVG viewBox is 0 0 80 200, humanoid height ~1.8m
// export function worldToSVG(pos: { x: number; y: number }) {
//   const svgX = 40 + (pos.x / 0.45) * 28;          // centre=40, clamp to body width
//   const svgY = 10 + (1 - pos.y / 1.8) * 175;      // head at top, feet at bottom
//   return { svgX, svgY };
// }

// export const PAIN_COLORS: Record<string, string> = {
//   sharp: "#E24B4A",
//   ache:  "#EF9F27",
//   stiff: "#378ADD",
// };

// // Add this to lib/painZoneMapper.ts

// // const SVG_W = 80;
// // const SVG_H = 200;

// /**
//  * Converts UV coordinates to SVG x/y.
//  * KEY: Unity uvY=0 is BOTTOM, SVG y=0 is TOP → invert Y.
//  */
// // export function uvToSVG(uvX: number, uvY: number) {
// //   return {
// //     svgX: uvX * SVG_W,
// //     svgY: (1 - uvY) * SVG_H,   // ← this is the critical fix
// //   };
// // }

// // export function mapToRegion(uvX: number, uvY: number): string {
// //   const match = REGION_RULES.find(
// //     (r) => uvX >= r.xMin && uvX <= r.xMax && uvY >= r.yMin && uvY <= r.yMax
// //   );
// //   return match?.name ?? "Unknown";
// // }

// export function processPainZones(painZones: PainZone[]): ProcessedZone[] {
//   const regionsMap: Record<string, ProcessedZone> = {};

//   painZones.forEach((zone) => {
//     if (!zone.worldPosition || typeof zone.worldPosition !== "object") return; // skip malformed

//     const label = worldToLabel(zone.worldPosition);
//     const { svgX, svgY } = worldToSVG(zone.worldPosition);
//     const color = PAIN_COLORS[zone.painType] ?? "#888780";

//     if (!regionsMap[label]) {
//       regionsMap[label] = { label, painType: zone.painType, intensity: zone.intensity, count: 1, svgX, svgY, color };
//     } else if (zone.intensity > regionsMap[label].intensity) {
//       Object.assign(regionsMap[label], { intensity: zone.intensity, painType: zone.painType, svgX, svgY, color });
//       regionsMap[label].count += 1;
//     }
//   });

//   return Object.values(regionsMap);
// }

// // export function mapToRegion(zone: PainZone) {
// //   const { uvX, uvY } = zone;

// //   const match = REGION_RULES.find(
// //     (rule) =>
// //       uvX >= rule.xMin &&
// //       uvX <= rule.xMax &&
// //       uvY >= rule.yMin &&
// //       uvY <= rule.yMax
// //   );

// //   return match ? match.name : "Unknown";
// // }

// // export function processPainZones(painZones: PainZone[]) {
// //   const regionsMap: Record<string, any> = {};

// //   painZones.forEach((zone) => {
// //     const region = mapToRegion(zone);

// //     if (!regionsMap[region]) {
// //       regionsMap[region] = {
// //         label: region,
// //         painType: zone.painType,
// //         intensity: zone.intensity,
// //         count: 1,
// //       };
// //     } else {
// //       // Aggregate
// //       regionsMap[region].intensity = Math.max(
// //         regionsMap[region].intensity,
// //         zone.intensity
// //       );
// //       regionsMap[region].count += 1;
// //     }
// //   });

// //   return Object.values(regionsMap);
// // }







/*
==================
==================
**/


export type ProcessedZone = {
  label: string;
  painType: string;
  intensity: number;
  count: number;
  svgX: number;
  svgY: number;
  color: string;
};

export const PAIN_COLORS: Record<string, string> = {
  sharp: "#E24B4A",
  ache:  "#EF9F27",
  stiff: "#378ADD",
};

// "right_hand" → "Right hand",  "lower_back" → "Lower back"
function formatBodyPart(bodyPart: string): string {
  return bodyPart
    .split("_")
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");
}

export function worldToSVG(pos: { x: number; y: number }) {
  const svgX = 40 + (pos.x / 0.45) * 28;
  const svgY = 10 + (1 - pos.y / 1.8) * 175;
  return { svgX, svgY };
}

export function processPainZones(painZones: PainZone[]): ProcessedZone[] {
  const regionsMap: Record<string, ProcessedZone> = {};

  painZones.forEach((zone) => {
    if (!zone.worldPosition || typeof zone.worldPosition !== "object") return;

    const label = formatBodyPart(zone.bodyPart);
    const { svgX, svgY } = worldToSVG(zone.worldPosition);
    const color = PAIN_COLORS[zone.painType] ?? "#888780";

    if (!regionsMap[label]) {
      regionsMap[label] = {
        label,
        painType: zone.painType,
        intensity: zone.intensity,
        count: 1,
        svgX,
        svgY,
        color,
      };
    } else if (zone.intensity > regionsMap[label].intensity) {
      Object.assign(regionsMap[label], { intensity: zone.intensity, painType: zone.painType, svgX, svgY, color });
      regionsMap[label].count += 1;
    }
  });

  return Object.values(regionsMap);
}