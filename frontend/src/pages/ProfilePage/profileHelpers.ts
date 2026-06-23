export function initials(name: string): string {
    if (!name) return '?';
    const parts = name.split(/[._\- ]+/).filter(Boolean);
    if (parts.length === 0) return name[0]?.toUpperCase() ?? '?';
    if (parts.length === 1) return parts[0]!.slice(0, 2).toUpperCase();
    return ((parts[0]![0] ?? '') + (parts[1]![0] ?? '')).toUpperCase();
}

export function shortDate(iso: string, locale = 'en'): string {
    if (!iso) return '—';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return iso;
    return new Intl.DateTimeFormat(locale, { day: '2-digit', month: 'short', year: 'numeric' })
        .format(d)
        .toUpperCase();
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function accountAge(iso: string, t: (k: string, o?: any) => string): string {
    if (!iso) return '—';
    const days = Math.floor((Date.now() - new Date(iso).getTime()) / 86400000);
    if (days < 1) return t('profile.accountAge.today');
    if (days === 1) return t('profile.accountAge.oneDay');
    if (days < 30) return t('profile.accountAge.days', { count: days });
    const months = Math.floor(days / 30);
    if (months < 12) return t('profile.accountAge.months', { count: months });
    const years = Math.floor(months / 12);
    const rem = months % 12;
    return rem > 0
        ? t('profile.accountAge.yearsMonths', { years, months: rem })
        : t('profile.accountAge.years', { years });
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
