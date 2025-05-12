<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Personal Information</h2>
    </div>

    <transition name="fade" mode="out-in">
      <!-- Loading -->
      <template v-if="loading">
        <div class="flex items-center justify-center py-12">
          <svg class="animate-spin h-8 w-8 text-gray-700" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z"/>
          </svg>
        </div>
      </template>

      <!-- Error -->
      <template v-else-if="error">
        <div class="bg-red-100 text-red-700 p-4 rounded">
          {{ error }}
        </div>
      </template>

      <!-- Profile -->
      <template v-else-if="user">
        <div class="space-y-6">
          <!-- Avatar + Name -->
          <div class="flex items-center space-x-4">
            <div class="w-24 h-24 rounded-full overflow-hidden ring-4 ring-gray-200">
              <img :src="imageSrc" @error="onError" alt="Profile" class="object-cover w-full h-full"/>
            </div>
            <p class="text-xl font-semibold text-gray-800">{{ user.name }}</p>
          </div>

          <!-- All Details -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Username</div>
              <div>{{ user.username }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Email</div>
              <div>{{ user.email }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Phone</div>
              <div>{{ user.phone }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Gender</div>
              <div>{{ capitalize(user.gender) }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Date of Birth</div>
              <div>{{ formatDate(user.dateOfBirth) }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Membership Status</div>
              <div>{{ user.membershipStatus }}</div>
            </div>
            <div class="flex items-center space-x-2">
              <div class="font-bold text-gray-700 w-32">Last Login</div>
              <div>{{ formatDate(user.lastLogin) }}</div>
            </div>
          </div>

          <!-- Role + Edit -->
          <div class="flex items-center space-x-4">
            <div class="font-bold text-gray-700 w-32">Role</div>
            <div class="flex-1">{{ capitalize(user.role) }}</div>
            <button @click="$emit('edit-profile')" class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
              Edit
            </button>
          </div>
        </div>
      </template>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';

const user = ref(null);
const loading = ref(true);
const error = ref(null);

// Build your base URL however you like
const apiBase =  process.env.VUE_APP_API_URL || 'https://localhost:5001';

const usePlaceholder = ref(false);
const imageSrc = computed(() => {
  if (user.value?.profilePicture && !usePlaceholder.value) {
    return user.value.profilePicture;
  }
  const initials = encodeURIComponent(
      (user.value?.name || 'User')
          .split(' ')
          .map(w => w[0])
          .join('')
          .substring(0, 2)
  );
  return `https://ui-avatars.com/api/?name=${initials}&background=dddddd&color=555555&size=256`;
});
function onError() {
  usePlaceholder.value = true;
}

const capitalize = s => s ? s.charAt(0).toUpperCase() + s.slice(1) : '';
const formatDate = iso => iso ? new Date(iso).toLocaleString() : '—';

onMounted(async () => {
  try {
    const token = localStorage.getItem('authToken');
    const headers = token ? { Authorization: `Bearer ${token}` } : {};
    const { data } = await axios.get(`${apiBase}/api/account/profile`, { headers });

    // if your backend returns { user: {…} }, this line will unwrap it;
    // otherwise it will set user.value to the DTO directly
    user.value = data.user ?? data;
  } catch (err) {
    console.error(err);
    error.value = err.response?.data?.message || 'Could not load your profile.';
  } finally {
    loading.value = false;
  }
});
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
.ring-gray-200 {
  --tw-ring-color: #e5e7eb;
}
</style>