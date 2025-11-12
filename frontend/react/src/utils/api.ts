import axios from "axios";

export async function getParticipants(roomId: string) {
  try {
    const response = await axios.get(`/api/room/${roomId}/participants`);
    return response.data;
  } catch {
    return [];
  }
}

export async function deleteUserFromRoom({
  userId,
  adminUserCode,
}: {
  userId: string;
  adminUserCode: string;
}) {
  try {
    const response = await axios.delete(
      `/api/users/${userId}?userCode=${adminUserCode}`,
    );
    if (response.status === 204) {
      return { success: true };
    }
    return response.data;
  } catch (error: unknown) {
    let errorMessage = "Delete failed.";
    if (
      typeof error === "object" &&
      error !== null &&
      "response" in error &&
      typeof (error as { response?: unknown }).response === "object" &&
      (error as { response?: { data?: { errorMessage?: string } } }).response
        ?.data?.errorMessage
    ) {
      errorMessage =
        (error as { response: { data: { errorMessage?: string } } }).response
          .data.errorMessage || errorMessage;
    } else if (error instanceof Error) {
      errorMessage = error.message;
    } else {
      errorMessage = String(error);
    }
    return {
      success: false,
      errorMessage,
    };
  }
}
