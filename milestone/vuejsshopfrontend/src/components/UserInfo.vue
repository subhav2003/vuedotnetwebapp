<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Personal Information</h2>

    </div>

    <transition name="fade" mode="out-in">
      <!-- loading -->
      <template v-if="!user">
        <!-- Loading Indicator -->
        <div  class="flex items-center justify-center py-12">
          <svg
              class="animate-spin h-8 w-8 text-gray-700"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
          >
            <circle
                class="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                stroke-width="4"
            ></circle>
            <path
                class="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8v8H4z"
            ></path>
          </svg>
          <span class="ml-3 text-gray-700 font-medium">Loading Personnel Info...</span>
        </div>
      </template>

      <!-- content -->
      <template v-else>

        <div class="space-y-6">

          <!-- Avatar + Name -->
          <div class="flex items-center space-x-4">
            <div class="w-24 h-24 rounded-full overflow-hidden ring-4 ring-gray-200">
              <img
                  :src="imageSrc"
                  @error="onError"
                  alt="Profile"
                  class="object-cover w-full h-full"
              />
            </div>
            <div>
              <p class="text-xl font-semibold text-gray-800">{{ user.name }}</p>
              <p class="text-gray-500">@{{ user.username }}</p>
            </div>
          </div>

          <!-- Details: 2 columns, each row is a label→value flex -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-4 text-gray-800">
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-24">Email:</div>
              <div class="flex-1 break-all">{{ user.email }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-24">Phone:</div>
              <div class="flex-1">{{ user.phone || '—' }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-24">Gender:</div>
              <div class="flex-1">{{ capitalize(user.gender) }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-24">Date of Birth:</div>
              <div class="flex-1">{{ formatDate(user.date_of_birth) }}</div>
            </div>
            <div class="flex items-center space-x-2 sm:col-span-2">
              <div class="font-bold text-gray-700 w-24">Role:</div>
              <div class="flex-1">{{ capitalize(user.role) }}</div>
              <button
                  @click="$emit('edit-profile')"
                  class="bg-gray-800 text-white px-4 py-2 rounded-md hover:bg-gray-900 transition"
              >
                Edit
              </button>
            </div>
          </div>
        </div>
      </template>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, watch, defineProps, defineEmits } from 'vue';

const props = defineProps({ user: { type: Object, default: null } });
const emit = defineEmits(['edit-profile']);

const usePlaceholder = ref(false);
const imageSrc = computed(() => {
  if (!usePlaceholder.value && props.user?.profile_picture) {
    return props.user.profile_picture;
  }
  const name = props.user?.name || 'User';
  const initials = encodeURIComponent(
      name
          .split(' ')
          .map(w => w[0])
          .join('')
          .substring(0, 2)
  );
  return `https://ui-avatars.com/api/?name=${initials}&background=dddddd&color=555555&size=256`;
});
function onError() { usePlaceholder.value = true; }
watch(() => props.user?.profile_picture, () => (usePlaceholder.value = false));

const capitalize = s => s ? s.charAt(0).toUpperCase() + s.slice(1) : '';
const formatDate = iso => iso ? new Date(iso).toLocaleDateString() : '—';
</script>

<style scoped>
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
.fade-enter-active,
.fade-leave-active {
  transition: all 0.3s ease;
}
/* preserve your custom ring color if you like */
.ring-gray-200 { --tw-ring-color: #e5e7eb; }
</style>
