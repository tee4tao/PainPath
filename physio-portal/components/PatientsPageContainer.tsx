"use client";
import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Session } from "@/types";
import StatRow from "./StatRow";

// ── Helpers ────────────────────────────────────────────────────────────────
const PAIN_COLORS = { sharp: "#E24B4A", ache: "#EF9F27", stiff: "#378ADD" };
const TABS = ["Pending review", "Approved", "All patients"];

function timeAgo(isoString: number) {
  const diff = Date.now() - new Date(isoString).getTime();
  const mins = Math.floor(diff / 60000);
  const hrs  = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);
  if (mins < 60)  return `${mins}m ago`;
  if (hrs  < 24)  return `${hrs}h ago`;
  return `${days}d ago`;
}

function StatusPill({ status }: { status: Session["status"] }) {
  if (status === "pending_review") {
    return (
      <span className="inline-flex items-center gap-1 text-[10px] font-medium px-2 py-[2px] rounded-full bg-[#FCEBEB] text-[#A32D2D]">
        <span className="w-[6px] h-[6px] rounded-full bg-[#E24B4A] inline-block" style={{ animation: "pulse 2s infinite" }} />
        Pending review
      </span>
    );
  }
  return (
    <span className="inline-flex items-center gap-1 text-[10px] font-medium px-2 py-[2px] rounded-full bg-[#E1F5EE] text-[#085041]">
      <span className="w-[6px] h-[6px] rounded-full bg-[#0F6E56] inline-block" />
      Approved
    </span>
  );
}

function PatientsCard({ patient }: { patient: Session }) {
  const isPending = patient.status === "pending_review";
  const regions   = patient.processed?.regions ?? [];
  const maxRegion = regions.reduce((a, b) => b.intensity > a.intensity ? b : a, regions[0]);

//   console.log(patient.sessionId);
  

  return (
    <Link
    //   onClick={onClick}
    href={`/patients/${patient.sessionId}`}
      className={`bg-white border rounded-xl p-4 cursor-pointer transition-all hover:shadow-md hover:-translate-y-[1px] active:translate-y-0 ${
        isPending ? "border-[#E24B4A]/20" : "border-black/[0.1]"
      }`}
    >
      {/* Top row */}
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-[#EEEDFE] flex items-center justify-center text-[12px] font-semibold text-[#3C3489] shrink-0">
            {patient.patientId.slice(-2)}
          </div>
          <div>
            <div className="text-[13px] font-medium text-[#2C2C2A]">{patient.patientId}</div>
            <div className="text-[11px] text-[#B4B2A9] mt-[1px]">
              {patient.sessionId.slice(0, 8)}… · {patient.deviceType}
            </div>
          </div>
        </div>
        <StatusPill status={patient.status} />
      </div>

      {/* Pain zones */}
      <div className="flex flex-wrap gap-[6px] mb-3">
        {regions.map((r) => (
          <span
            key={r.label}
            className="inline-flex items-center gap-1 text-[11px] px-2 py-[3px] rounded-full border"
            style={{
              borderColor: `${PAIN_COLORS[r.painType]}40`,
              background: `${PAIN_COLORS[r.painType]}12`,
              color: PAIN_COLORS[r.painType],
            }}
          >
            <span
              className="w-[6px] h-[6px] rounded-full inline-block shrink-0"
              style={{ background: PAIN_COLORS[r.painType] }}
            />
            {r.label} · {r.intensity}/10
          </span>
        ))}
      </div>

      {/* AI match */}
      {patient.aiAnalysis && (
        <div className="bg-[#FAEEDA] rounded-lg px-3 py-2 mb-3">
          <div className="text-[11px] text-[#854F0B]">
            <span className="font-medium">{patient.aiAnalysis.conditionMatch}</span>
            <span className="text-[#B4874A] ml-2">{patient.aiAnalysis.confidence}% confidence</span>
          </div>
        </div>
      )}

      {/* Footer */}
      <div className="flex items-center justify-between">
        <span className="text-[11px] text-[#B4B2A9]">Submitted {timeAgo(patient.submittedAt._seconds * 1000)}</span>
        {isPending && (
          <span className="text-[11px] font-medium text-[#0F6E56] flex items-center gap-1">
            Review session →
          </span>
        )}
      </div>
    </Link>
  );
}

const PatientsPageContainer = ({ sessions }: { sessions: Session[] }) => {

      const router = useRouter();
  const [activeTab, setActiveTab] = useState(0);

  const pending  = sessions.filter((p) => p.status === "pending_review");
  const approved = sessions.filter((p) => p.status === "approved");
  const all      = sessions;

  const lists = [pending, approved, all];
  const displayed = lists[activeTab];

  const pendingCount = pending.length;
  const approvedCount = approved.length;
  return (
        <div className="min-h-screen py-4" style={{ background: "#F1EFE8" }}>

      {/* Tabs */}
      <div className="flex gap-1 bg-white border border-black/[0.1] rounded-xl p-1 mb-4 w-fit">
        {TABS.map((tab, i) => (
          <button
            key={tab}
            onClick={() => setActiveTab(i)}
            className={`px-4 py-[7px] rounded-lg text-[12px] font-medium transition-all flex items-center gap-2 ${
              activeTab === i
                ? "bg-[#0F6E56] text-white shadow-sm"
                : "text-[#888780] hover:text-[#2C2C2A] hover:bg-[#F1EFE8]"
            }`}
          >
            {tab}
            {i === 0 && pendingCount > 0 && (
              <span
                className={`text-[10px] px-[6px] py-[1px] rounded-full font-semibold ${
                  activeTab === 0 ? "bg-white/20 text-white" : "bg-[#FCEBEB] text-[#A32D2D]"
                }`}
              >
                {pendingCount}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Patient list */}
      {displayed.length === 0 ? (
        <div className="bg-white border border-black/[0.1] rounded-xl p-10 text-center">
          <div className="text-[13px] text-[#888780]">No patients in this category</div>
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-3 md:grid-cols-2">
          {displayed.map((patient) => (
            <PatientsCard
              key={patient.sessionId}
              patient={patient}
            //   onClick={() => router.push(`/patients/${patient.sessionId}`)}
            />
          ))}
        </div>
      )}

      <style>{`
        @keyframes pulse { 0%,100%{opacity:1} 50%{opacity:0.4} }
      `}</style>
    </div>
  )
}

export default PatientsPageContainer