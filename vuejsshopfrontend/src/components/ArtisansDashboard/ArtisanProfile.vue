<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">My Artisan Profile</h2>
      <button
          @click="$emit('edit-artisan')"
          class="bg-gray-800 text-white px-4 py-2 rounded-md hover:bg-gray-900 transition-colors"
      >
        Edit
      </button>
    </div>

    <transition name="fade" mode="out-in">
      <!-- Loading State -->
      <template v-if="!artisan">
        <div class="flex flex-col items-center justify-center py-12">
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
          <span class="mt-3 text-gray-700 font-medium">Loading Profile...</span>
        </div>
      </template>

      <!-- Details -->
      <template v-else>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-4 text-gray-800">
          <div class="flex items-start">
            <span class="w-32 font-semibold">ID:</span>
            <span>{{ artisan.id }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Name:</span>
            <span>{{ artisan.name }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Bio:</span>
            <span class="flex-1 break-all">{{ artisan.bio || 'N/A' }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Skills:</span>
            <span class="flex-1 break-all">{{ artisan.skills || 'N/A' }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Portfolio:</span>
            <span class="flex-1 break-all">{{ artisan.portfolio_url || 'N/A' }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Location:</span>
            <span>{{ artisan.location || 'N/A' }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Lead Time:</span>
            <span>{{ artisan.lead_time ?? 'N/A' }} days</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Certifications:</span>
            <span class="flex-1 break-all">{{ artisan.certifications || 'N/A' }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Experience:</span>
            <span>{{ artisan.years_of_experience ?? 'N/A' }} yrs</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Custom Orders:</span>
            <span>{{ artisan.accept_custom_orders ? 'Yes' : 'No' }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Avg Rating:</span>
            <span class="flex items-center">
              {{ artisan.average_rating.toFixed(1) }}
              <font-awesome-icon :icon="faStar" class="text-yellow-400 ml-2" />
            </span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Created At:</span>
            <span>{{ artisan.created_at }}</span>
          </div>
          <div class="flex items-start">
            <span class="w-32 font-semibold">Updated At:</span>
            <span>{{ artisan.updated_at }}</span>
          </div>
          <!-- Social Links full width -->
          <div class="flex items-start sm:col-span-2">
            <span class="w-32 font-semibold">Social Links:</span>
            <div class="flex-1 space-y-1">
              <p v-if="social.facebook">
                <a :href="social.facebook" target="_blank" class="text-blue-600 hover:underline">Facebook</a>
              </p>
              <p v-if="social.twitter">
                <a :href="social.twitter" target="_blank" class="text-blue-600 hover:underline">Twitter</a>
              </p>
              <p v-if="social.instagram">
                <a :href="social.instagram" target="_blank" class="text-blue-600 hover:underline">Instagram</a>
              </p>
              <p v-if="!social.facebook && !social.twitter && !social.instagram">N/A</p>
            </div>
          </div>
        </div>
      </template>
    </transition>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue';
import { faStar } from '@fortawesome/free-solid-svg-icons';

const emit = defineEmits(['edit-artisan']);
const artisan = ref(null);
const apiBaseUrl = process.env.VUE_APP_API_URL;

onMounted(async () => {
  try {
    const token = localStorage.getItem('authToken');
    const res = await fetch(`${apiBaseUrl}/api/v1/artisan/profileinfo`, {
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' }
    });
    const data = await res.json();
    artisan.value = data.data || data;
  } catch (e) {
    console.error(e);
  }
});

const social = computed(() => {
  if (!artisan.value?.social_links) return {};
  try { return JSON.parse(artisan.value.social_links) } catch { return {} }
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
</style>
