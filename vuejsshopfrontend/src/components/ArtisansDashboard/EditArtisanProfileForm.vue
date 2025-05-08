<template>
  <!-- Full-screen overlay -->
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <!-- Modal Container -->
    <div class="bg-white p-6 rounded-lg w-full max-w-lg max-h-[90vh] overflow-y-auto">
      <h2 class="text-xl font-bold mb-4">Edit Artisan Profile</h2>

      <!-- API Message -->
      <div
          v-if="apiMessage"
          class="mb-4 p-2 rounded text-center"
          :class="{
          'bg-green-100 text-green-700': success,
          'bg-red-100 text-red-700': !success
        }"
      >
        {{ apiMessage }}
      </div>

      <!-- Loading Indicator -->
      <div v-if="loading" class="py-4">
        <p class="text-center text-gray-600">Loading...</p>
      </div>

      <!-- Main Form: Only show when form data is ready -->
      <form v-else-if="form" @submit.prevent="updateArtisan" class="space-y-4 text-sm">
        <!-- Bio Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="bio">Bio</label>
          <textarea id="bio" v-model="form.bio" class="input-field" rows="3"></textarea>
        </div>

        <!-- Skills Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="skills">Skills</label>
          <input id="skills" v-model="form.skills" type="text" class="input-field" />
        </div>

        <!-- Portfolio URL Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="portfolio_url">Portfolio URL</label>
          <input id="portfolio_url" v-model="form.portfolio_url" type="url" class="input-field" />
        </div>

        <!-- Location Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="location">Location</label>
          <input id="location" v-model="form.location" type="text" class="input-field" />
        </div>

        <!-- Lead Time Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="lead_time">Lead Time (days)</label>
          <input id="lead_time" v-model="form.lead_time" type="number" class="input-field" />
        </div>

        <!-- Certifications Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="certifications">Certifications</label>
          <input
              id="certifications"
              v-model="form.certifications"
              type="text"
              class="input-field"
          />
        </div>

        <!-- Years of Experience Field -->
        <div>
          <label class="block text-gray-700 mb-1" for="years_of_experience">Years of Experience</label>
          <input
              id="years_of_experience"
              v-model="form.years_of_experience"
              type="number"
              class="input-field"
          />
        </div>

        <!-- Social Links Section -->
        <div>
          <label class="block text-gray-700 mb-2">Social Links</label>
          <!-- Facebook -->
          <div class="mb-2">
            <label for="facebook" class="block text-gray-700">Facebook URL</label>
            <input id="facebook" type="url" v-model="form.facebook" class="input-field" />
          </div>
          <!-- Twitter -->
          <div class="mb-2">
            <label for="twitter" class="block text-gray-700">Twitter URL</label>
            <input id="twitter" type="url" v-model="form.twitter" class="input-field" />
          </div>
          <!-- Instagram -->
          <div class="mb-2">
            <label for="instagram" class="block text-gray-700">Instagram URL</label>
            <input id="instagram" type="url" v-model="form.instagram" class="input-field" />
          </div>
        </div>

        <!-- Custom Order Status Toggle -->
        <div>
          <label class="block text-gray-700 mb-1">Accept Custom Orders:</label>
          <label class="inline-flex relative items-center cursor-pointer">
            <!-- Store as boolean in the form -->
            <input
                type="checkbox"
                v-model="form.accept_custom_orders"
                class="sr-only peer"
            />
            <div
                class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-2 peer-focus:ring-blue-500
                     rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white
                     after:content-[''] after:absolute after:top-0.5 after:left-[2px] after:bg-white
                     after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all
                     dark:border-gray-600 peer-checked:bg-blue-600"
            ></div>
            <!-- Show Yes or No to confirm value -->
            <span class="ml-3 text-sm font-medium text-gray-900">
              {{ form.accept_custom_orders ? 'Yes' : 'No' }}
            </span>
          </label>
        </div>

        <!-- Form Actions -->
        <div class="flex justify-end gap-4 mt-4">
          <button
              type="button"
              @click="closeForm"
              class="bg-gray-300 text-gray-700 py-2 px-4 rounded"
          >
            Cancel
          </button>
          <button type="submit" :disabled="loading" class="bg-blue-600 text-white py-2 px-4 rounded">
            <span v-if="loading">Updating...</span>
            <span v-else>Update Artisan Profile</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import axios from 'axios';

// Emitted events
const emit = defineEmits(['close', 'updated']);

// Config
const apiBaseUrl = process.env.VUE_APP_API_URL;

// State
const loading = ref(false);
const apiMessage = ref('');
const success = ref(false);
const form = ref(null);

// Fetch artisan data
const fetchArtisanData = async () => {
  try {
    loading.value = true;
    const token = localStorage.getItem('authToken');
    const response = await axios.get(`${apiBaseUrl}/api/v1/artisan/profileinfo`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    const artisan = response.data.data || response.data;
    // The server returns "accept_custom_order" as a boolean
    // but we want to store it in "accept_custom_orders"
    // for the checkbox in the form.
    form.value = {
      bio: artisan.bio || '',
      skills: artisan.skills || '',
      portfolio_url: artisan.portfolio_url || '',
      location: artisan.location || '',
      lead_time: artisan.lead_time || '',
      certifications: artisan.certifications || '',
      years_of_experience: artisan.years_of_experience || '',
      // Social links
      facebook: '',
      twitter: '',
      instagram: '',
      // Convert the server's "accept_custom_order" => boolean
      accept_custom_orders: artisan.accept_custom_order === true,
    };

    // Parse existing social_links if present
    if (artisan.social_links) {
      try {
        const links = JSON.parse(artisan.social_links);
        form.value.facebook = links.facebook || '';
        form.value.twitter = links.twitter || '';
        form.value.instagram = links.instagram || '';
      } catch (e) {
        console.error('Error parsing social_links:', e);
      }
    }
  } catch (error) {
    console.error('Error fetching artisan profile:', error);
  } finally {
    loading.value = false;
  }
};
onMounted(() => {
  fetchArtisanData();
});

// Update artisan profile
const updateArtisan = async () => {
  try {
    loading.value = true;

    const updateData = new FormData();
    updateData.append('_method', 'PUT');

    // Convert booleans to 1 or 0 before sending
    Object.entries(form.value).forEach(([key, value]) => {
      if (key === 'accept_custom_orders') {
        updateData.append(key, value ? 1 : 0);
      } else {
        updateData.append(key, value);
      }
    });

    const token = localStorage.getItem('authToken');
    const response = await axios.post(`${apiBaseUrl}/api/v1/artisan/update`, updateData, {
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'multipart/form-data',
      },
    });

    apiMessage.value = response.data.message || 'Artisan profile updated successfully.';
    success.value = true;
    emit('updated', response.data);
  } catch (error) {
    console.error('Error updating artisan profile:', error);
    apiMessage.value = error.response?.data?.message || 'Failed to update artisan profile.';
    success.value = false;
  } finally {
    loading.value = false;
    // Close after 2 seconds if successful
    if (success.value) {
      setTimeout(() => {
        closeForm();
      }, 2000);
    }
  }
};

const closeForm = () => {
  emit('close');
};
</script>

<style scoped>
.input-field {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.375rem;
  margin-top: 0.25rem;
  outline: none;
  transition: border-color 0.2s;
}
.input-field:focus {
  border-color: #3b82f6;
}

/* Simple spinner styling */
.loader {
  text-align: center;
}
</style>
