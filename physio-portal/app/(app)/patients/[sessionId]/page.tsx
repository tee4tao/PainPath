import PatientPageContainer from "@/components/PatientPageContainer";
import StatRow from "@/components/StatRow";

export default  async function PatientsPage({params}: { params: Promise<{ sessionId: string }> }) {
    const { sessionId } = await params;
  
  const res = await fetch(
    `http://localhost:3000/api/sessions/${sessionId}`,
    { cache: "no-store" }
  );

  const result = await res.json();

//   console.log(data);
  

  return (
    <>
    <StatRow />
    <PatientPageContainer result={result} />
    </>
  );
}
