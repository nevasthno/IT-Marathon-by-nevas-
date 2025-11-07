export async function getParticipants(roomId: string) {
    try {
        const response = await axios.get(`/api/room/${roomId}/participants`);
        return response.data;
    } catch (error: any) {
        return [];
    }
}
import axios from "axios";

export async function deleteUserFromRoom({ userId, adminUserCode }: { userId: string; adminUserCode: string }) {
    try {
        const response = await axios.delete(`/api/room/user`, {
            data: { userId, adminUserCode },
        });
        return response.data;
    } catch (error: any) {
        return {
            success: false,
            errorMessage: error?.response?.data?.errorMessage || error.message,
        };
    }
}
