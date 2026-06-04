export function initials(name: string): string {
    if (!name) return '?';
    const parts = name.split(/[._\- ]+/).filter(Boolean);
    if (parts.length === 0) return name[0]?.toUpperCase() ?? '?';
    if (parts.length === 1) return parts[0]!.slice(0, 2).toUpperCase();
    return ((parts[0]![0] ?? '') + (parts[1]![0] ?? '')).toUpperCase();
}

export function shortDate(iso: string): string {
    if (!iso) return '—';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return iso;
    const months = ['JAN','FEB','MAR','APR','MAY','JUN','JUL','AUG','SEP','OCT','NOV','DEC'] as const;
    return `${String(d.getDate()).padStart(2, '0')} ${months[d.getMonth() as 0|1|2|3|4|5|6|7|8|9|10|11]} ${d.getFullYear()}`;
}

export function accountAge(iso: string): string {
    if (!iso) return '—';
    const days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
    if (days < 1) return 'TODAY';
    if (days === 1) return '1 DAY';
    if (days < 30) return `${days} DAYS`;
    const months = Math.floor(days / 30);
    if (months < 12) return `${months} MO`;
    const years = Math.floor(months / 12);
    const rem = months % 12;
    return rem > 0 ? `${years}Y ${rem}MO` : `${years}Y`;
}

export function banRemain(iso: string): string {
    const diff = new Date(iso).getTime() - Date.now();
    if (diff <= 0) return '0H';
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(hours / 24);
    if (days > 0) return `${days}D ${hours % 24}H`;
    const mins = Math.floor((diff / 60000) % 60);
    return `${hours}H ${mins}M`;
}
