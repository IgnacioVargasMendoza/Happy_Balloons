import React from 'react';
import { Outlet } from 'react-router';
import { Header } from './Header';
import { Toaster } from './ui/sonner';

export function Layout() {
  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="container mx-auto px-4 py-6 md:py-8">
        <Outlet />
      </main>
      <Toaster position="top-center" richColors />
    </div>
  );
}
