import Navbar from "@/components/Navbar";


export default function LayoutPublic({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-full flex flex-col p-4">
          <Navbar/>
          {children}
      </div>
  );
}
