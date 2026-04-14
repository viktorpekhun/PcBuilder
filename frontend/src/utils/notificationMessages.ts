import type { NotificationType, INotificationPayload } from "../types/notification.types";

type MessageRenderer = (payload: INotificationPayload) => string;

const messageTemplates: Record<NotificationType, MessageRenderer> = {
  ReviewDeleted: (p) =>
    `Your review on "${p.buildName ?? "a build"}" was removed by a moderator.`,

  BuildDeleted: (p) =>
    `Your build "${p.buildName ?? "Unknown"}" was removed by a moderator.`,

  CommentBanned: (p) => {
    const until = p.banUntil ? new Date(p.banUntil).toLocaleDateString() : "unknown date";
    return `You have been banned from commenting until ${until}. Reason: ${p.reason ?? "No reason provided."}`;
  },

  PostBanned: (p) => {
    const until = p.banUntil ? new Date(p.banUntil).toLocaleDateString() : "unknown date";
    return `You have been banned from posting until ${until}. Reason: ${p.reason ?? "No reason provided."}`;
  },

  CommentWarning: (p) =>
    `You received a comment warning (${p.warningsCount ?? "?"} total). Reason: ${p.reason ?? "No reason provided."}`,

  PostWarning: (p) =>
    `You received a post warning (${p.warningsCount ?? "?"} total). Reason: ${p.reason ?? "No reason provided."}`,

  NewReview: (p) =>
    `${p.reviewerUsername ?? "Someone"} left a review on your build "${p.buildName ?? "a build"}".`,
};

export function renderNotificationText(
  type: string,
  payload: INotificationPayload
): string {
  const renderer = messageTemplates[type as NotificationType];
  if (renderer) {
    return renderer(payload);
  }
  return "You have a new notification.";
}
