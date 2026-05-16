'use client';

import React from 'react';
import { getInitials } from '@/lib/utils';

const AVATAR_COLORS = [
  'bg-blue-500', 'bg-purple-500', 'bg-teal-500', 'bg-amber-500',
  'bg-pink-500', 'bg-indigo-500', 'bg-cyan-500', 'bg-emerald-500',
];

function getColorForName(name: string): string {
  let hash = 0;
  for (let i = 0; i < name.length; i++) {
    hash = name.charCodeAt(i) + ((hash << 5) - hash);
  }
  return AVATAR_COLORS[Math.abs(hash) % AVATAR_COLORS.length];
}

interface AvatarProps {
  name: string;
  src?: string | null;
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl';
}

const sizeMap = {
  xs: { container: 'w-6 h-6', text: 'text-[9px]' },
  sm: { container: 'w-8 h-8', text: 'text-xs' },
  md: { container: 'w-10 h-10', text: 'text-sm' },
  lg: { container: 'w-12 h-12', text: 'text-base' },
  xl: { container: 'w-16 h-16', text: 'text-xl' },
};

export function Avatar({ name, src, size = 'md' }: AvatarProps) {
  const s = sizeMap[size];
  const color = getColorForName(name);

  if (src) {
    return (
      <img
        src={src}
        alt={name}
        className={`${s.container} rounded-full object-cover flex-shrink-0 border-2 border-white`}
      />
    );
  }

  return (
    <div
      className={`${s.container} ${color} rounded-full flex items-center justify-center flex-shrink-0 border-2 border-white`}
    >
      <span className={`${s.text} font-semibold text-white leading-none`}>
        {getInitials(name)}
      </span>
    </div>
  );
}
