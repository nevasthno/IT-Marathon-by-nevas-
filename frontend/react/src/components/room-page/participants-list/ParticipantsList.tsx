import { useState } from "react";
import { useCallback } from "react";
import { useParams } from "react-router";
import { deleteUserFromRoom } from "../../../utils/api";
import ParticipantCard from "@components/common/participant-card/ParticipantCard";
import ParticipantDetailsModal from "@components/common/modals/participant-details-modal/ParticipantDetailsModal";
import type { Participant } from "../../../types/api";
import {
  MAX_PARTICIPANTS_NUMBER,
  generateParticipantLink,
} from "@utils/general";
import { type ParticipantsListProps, type PersonalInformation } from "./types";
import "./ParticipantsList.scss";

const ParticipantsList = ({
  participants,
  onUserDeleted,
  isRandomized,
}: ParticipantsListProps & {
  onUserDeleted?: () => void;
  isRandomized?: boolean;
}) => {
  const { userCode } = useParams();
  const [deletingUserCode, setDeletingUserCode] = useState<string | null>(null);
  const [showRefreshTip, setShowRefreshTip] = useState(false);
  const handleDeleteUser = useCallback(
    async (userIdToDelete: number) => {
      if (!userCode || !userIdToDelete) return;
      setDeletingUserCode(userIdToDelete.toString());
      try {
        const res = await deleteUserFromRoom({
          userId: userIdToDelete.toString(),
          adminUserCode: userCode,
        });
        if (
          res === undefined ||
          res === null ||
          (typeof res === "object" && Object.keys(res).length === 0) ||
          res.success
        ) {
          let attempts = 0;
          const maxAttempts = 5;
          const delay = 300;
          const retryFetch = async () => {
            if (onUserDeleted) await onUserDeleted();
            attempts++;
            setTimeout(async () => {
              if (attempts < maxAttempts) {
                setShowRefreshTip(true);
              }
            }, delay);
          };
          retryFetch();
        } else {
          alert(res.errorMessage || "Не вдалося видалити користувача");
        }
      } catch {
        alert("Помилка при видаленні користувача");
      }
      setDeletingUserCode(null);
    },
    [userCode, onUserDeleted],
  );
  const [selectedParticipant, setSelectedParticipant] =
    useState<PersonalInformation | null>(null);

  const admin = participants?.find((participant) => participant?.isAdmin);
  const restParticipants = participants?.filter(
    (participant) => !participant?.isAdmin,
  );

  const isParticipantsMoreThanTen = participants.length > 10;

  const handleInfoButtonClick = (participant: Participant) => {
    const personalInfoData: PersonalInformation = {
      firstName: participant.firstName,
      lastName: participant.lastName,
      phone: participant.phone,
      deliveryInfo: participant.deliveryInfo,
      email: participant.email,
      link: generateParticipantLink(participant.userCode),
    };
    setSelectedParticipant(personalInfoData);
  };

  const handleModalClose = () => setSelectedParticipant(null);

  return (
    <div
      className={`participant-list ${isParticipantsMoreThanTen ? "participant-list--shift-bg-image" : ""}`}
    >
      <div
        className={`participant-list__content ${isParticipantsMoreThanTen ? "participant-list__content--extra-padding" : ""}`}
      >
        <div className="participant-list-header">
          <h3 className="participant-list-header__title">Who’s Playing?</h3>

          <span className="participant-list-counter__current">
            {participants?.length ?? 0}/
          </span>

          <span className="participant-list-counter__max">
            {MAX_PARTICIPANTS_NUMBER}
          </span>
        </div>

        <div className="participant-list__cards">
          {admin ? (
            <ParticipantCard
              key={admin?.id}
              firstName={admin?.firstName}
              lastName={admin?.lastName}
              isCurrentUser={userCode === admin?.userCode}
              isAdmin={admin?.isAdmin}
              isCurrentUserAdmin={userCode === admin?.userCode}
              adminInfo={`${admin?.phone}${admin?.email ? `\n${admin?.email}` : ""}`}
              participantLink={generateParticipantLink(admin?.userCode)}
            />
          ) : null}

          {restParticipants?.map((user) => (
            <div
              key={user?.userCode}
              className="participant-list__card-wrapper"
            >
              <ParticipantCard
                firstName={user?.firstName}
                lastName={user?.lastName}
                isCurrentUser={userCode === user?.userCode}
                isCurrentUserAdmin={userCode === admin?.userCode}
                participantLink={generateParticipantLink(user?.userCode)}
                onInfoButtonClick={
                  userCode === admin?.userCode && userCode !== user?.userCode
                    ? () => handleInfoButtonClick(user)
                    : undefined
                }
                onDeleteButtonClick={
                  !isRandomized &&
                  userCode === admin?.userCode &&
                  userCode !== user?.userCode
                    ? () => handleDeleteUser(user.id)
                    : null
                }
                isDeleting={deletingUserCode === user?.id.toString()}
              />
            </div>
          ))}
        </div>

        {showRefreshTip && (
          <div
            className="participant-list__refresh-tip"
            style={{ color: "#ff4d4f", margin: "16px 0", textAlign: "center" }}
          >
            Дані не оновились? Будь ласка, <b>оновіть сторінку</b> вручну.
          </div>
        )}

        {selectedParticipant ? (
          <ParticipantDetailsModal
            isOpen={!!selectedParticipant}
            onClose={handleModalClose}
            personalInfoData={selectedParticipant}
          />
        ) : null}
      </div>
    </div>
  );
};

export default ParticipantsList;
