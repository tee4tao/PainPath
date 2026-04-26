"use client";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleLogin(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    if (!email || !password) {
      setError("Please enter your email and password.");
      return;
    }
    setLoading(true);
    // TODO: replace with real Firebase Auth
  await new Promise((r) => setTimeout(r, 1000));

  // Set the cookie so middleware can read it
  document.cookie = "isLoggedIn=true; path=/; max-age=86400"; // expires in 24hrs

    setLoading(false);
    router.push("/");
  }

  useEffect(() => {
  const isLoggedIn = document.cookie.includes("isLoggedIn=true");
  if (isLoggedIn) {
    router.push("/patients");
  }
}, [router]);

  return (
    <div className="h-screen flex items-center justify-center">
      <div className="w-full max-w-sm">

        {/* Logo */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center gap-2 mb-3">
            <div className="w-8 h-8 rounded-lg bg-[#0F6E56] flex items-center justify-center">
              <svg viewBox="0 0 20 20" fill="none" className="w-4 h-4">
                <path d="M10 3C10 3 4 7 4 12a6 6 0 0012 0C16 7 10 3 10 3z" fill="white" opacity="0.9"/>
                <circle cx="10" cy="12" r="2" fill="white"/>
              </svg>
            </div>
            <span className="text-[18px] font-semibold text-[#2C2C2A]">PainPath</span>
          </div>
          <p className="text-[13px] text-[#888780]">Physio Portal — sign in to continue</p>
        </div>

        {/* Card */}
        <div className="bg-white border border-black/[0.1] rounded-2xl p-6 shadow-sm">
          <form onSubmit={handleLogin} className="flex flex-col gap-4">

            <div className="flex flex-col gap-1">
              <label className="text-[11px] font-medium text-[#888780] uppercase tracking-[0.05em]">
                Email address
              </label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@nhs.net"
                className="w-full px-3 py-[10px] rounded-lg border border-black/[0.12] text-[13px] text-[#2C2C2A] bg-[#F1EFE8] placeholder:text-[#B4B2A9] outline-none focus:border-[#0F6E56] focus:ring-2 focus:ring-[#0F6E56]/10 transition-all"
              />
            </div>

            <div className="flex flex-col gap-1">
              <label className="text-[11px] font-medium text-[#888780] uppercase tracking-[0.05em]">
                Password
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full px-3 py-[10px] rounded-lg border border-black/[0.12] text-[13px] text-[#2C2C2A] bg-[#F1EFE8] placeholder:text-[#B4B2A9] outline-none focus:border-[#0F6E56] focus:ring-2 focus:ring-[#0F6E56]/10 transition-all"
              />
            </div>

            {error && (
              <p className="text-[12px] text-[#A32D2D] bg-[#FCEBEB] px-3 py-2 rounded-lg">
                {error}
              </p>
            )}

            <button
              type="submit"
              disabled={loading}
              className="w-full py-[11px] rounded-lg bg-[#0F6E56] text-[#E1F5EE] text-[13px] font-medium hover:bg-[#085041] disabled:opacity-60 disabled:cursor-not-allowed transition-colors mt-1"
            >
              {loading ? "Signing in…" : "Sign in"}
            </button>

          </form>

          <div className="mt-4 pt-4 border-t border-black/[0.08] text-center">
            <a href="#" className="text-[12px] text-[#0F6E56] hover:underline">
              Forgot password?
            </a>
          </div>
        </div>

        <p className="text-center text-[11px] text-[#B4B2A9] mt-6">
          Access restricted to authorised NHS physiotherapists
        </p>
      </div>
    </div>
  );
}
